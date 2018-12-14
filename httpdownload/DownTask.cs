using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace comicDownLoad
{
    public enum DownLoadStatus
    { 
        DownLoading,
        Pause,
        TimeOut,
        Resume,
        Finish
    }

    class DownLoadArgs:EventArgs//下载事件
    {
        private bool downFinished;
        private long fileSize;
        private string taskName;
        private List<string> m_urlList;
        private int progress;
        public List<string> UrlList { get { return m_urlList; } set { m_urlList = value; } }
        public bool DowmFinished { get { return downFinished; } set { downFinished = value; } }
        public string TaskName { get { return taskName; } set { taskName = value;} }
        public long FileSize { get { return fileSize; } set { fileSize = value; } }
        public int DownLoadProgress { get { return progress; } set { progress = value; } }

        public DownLoadArgs(bool finished)
        {
            downFinished = finished;
        }

        public DownLoadArgs(bool finished, string taskName, int progress, List<string> urlList, long fileLen)
        {
            downFinished = finished;
            this.taskName = taskName;
            m_urlList = urlList;
            this.progress = progress;
            fileSize = fileLen;
        }
    }

    class DownTask//下载任务类
    {
        private string m_path;
        private DownLoadStatus m_status;
        private List<string>m_fileList;
        public string ComicName { get; set; }
        public string ReferUrl { get; set; }
        public string DownLoadPath { get { return m_path; } set { m_path = value; } }
        public DownLoadStatus Status { get { return m_status; } set{m_status = value;}}
        public List<string> ImageFiles { get { return m_fileList; } set { m_fileList = value; } }
       
        public delegate void DownLoadOneFinished(object sender, DownLoadArgs args);
        public delegate void DownLoadEAllFinishedtHandler(object sender, DownLoadArgs args);
        public delegate void DownLoadPause(object sender, DownLoadArgs args);
        public delegate void DownResume(object sender, DownLoadArgs args);
        public delegate void AddPathListHandler(object sender, string fileName);

        public event DownLoadOneFinished downLoadOneFinished;//下载完成一张图片
        public event DownLoadEAllFinishedtHandler downFinished;//下载完成
        public event DownLoadPause downPaused;//暂停下载事件
        public event DownResume downResume;//继续下载事件
        public event AddPathListHandler addFileEvent;


        public DownTask()
        {
            m_fileList = new List<string>();
        }

        private void SaveDownProgress()
        {
        
        }

        private void ResumeDownProgress()
        {
            
        }

        public void GetNetComic(DownLoadFile downFile)
        {
            Task task = new Task(() =>
            {
                FileStream file;
                var path = "";
                var decoder = DecoderDistrution.GiveDecoder(downFile.ComicUrl);
                var response = AnalyseTool.HttpGet(downFile.ComicUrl);
                var ds = decoder.GetDownImageList(response);
                path = DownLoadPath + "\\" + ComicName + "\\";

                if (File.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }

                int errorCount = 0;
                int read = 0;
                int count = 0;

                foreach (var i in ds.ImageList)
                {
                    file = new FileStream(path + (count++) + ".jpg", FileMode.Create);
                    var stream = DownLoad(i, ReferUrl);

                    while (stream == null && errorCount < 3)
                    {
                        Thread.Sleep(100);
                        stream = DownLoad(i, ReferUrl);
                        errorCount++;
                    }

                    byte[] buffer = new byte[32768];

                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        file.Write(buffer, 0, read);
                    }

                    if (addFileEvent != null)
                        addFileEvent(this, file.Name);

                    file.Flush();
                    stream.Flush();
                    file.Close();                 
                    stream.Close();

                    
                }

            });

            task.Start();
        }

        public void DownLoadStart()//异步下载下载漫画
        {
            var read = 0;
            var pos = 0;
            var errorCount = 0;
            long fileLen = 0;
            var fileName = "";

            Task task = new Task(() =>
            {               
                int downProgress = 0;

                foreach (var i in m_fileList)
                {
                    if(m_status == DownLoadStatus.Pause)
                    {
                        break;
                    }

                    Stream retStream = DownLoad(i, ReferUrl);//下载url
                    errorCount = 0;

                    while (retStream == null && errorCount < 3)
                    {
                        Thread.Sleep(100);
                        retStream = DownLoad(i, ReferUrl);
                        errorCount++;
                    }

                    if (retStream == null)
                    {
                        downPaused(this, new DownLoadArgs(false, ComicName, downProgress, m_fileList, fileLen));//操作超时，下载暂停
                        return;
                        
                    }

                    if (i.Contains("webp") == false)
                        fileName = DownLoadPath + (pos++).ToString() + ".jpg";
                    else
                       fileName =  DownLoadPath + (pos++).ToString() + ".webp";

                    FileStream file = new FileStream(fileName, FileMode.Create);
                    byte[] buffer = new byte[32768];

                    while ((read = retStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        file.Write(buffer, 0, read);
                    }

                    fileLen = file.Length;
                    file.Flush();
                    retStream.Flush();
                    file.Close();
                    retStream.Close();
                    downProgress++;
                    downLoadOneFinished(this, new DownLoadArgs(true, ComicName, m_fileList.Count, m_fileList, fileLen));
                    Thread.Sleep(100);
                }
                
                if(m_status != DownLoadStatus.Pause)
                {
                    downFinished(this, new DownLoadArgs(true, ComicName, m_fileList.Count, m_fileList, fileLen));//触发下载完成事件
                }
                else
                {
                    downPaused(this, new DownLoadArgs(false, ComicName, downProgress, m_fileList, fileLen));
                }

            });

            task.Start();
            m_status = DownLoadStatus.DownLoading;
        }

        public Stream DownLoad(string url, string referUrl)//下载模块，添加gzip支持
        {
            Stream imgstream = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = "image/webp,image/*,*/*;q=0.8";
            request.Method = "GET";
            request.KeepAlive = true;
            request.Host = new Uri(url).Host;
            request.Headers.Add("Accept-Encoding:gzip, deflate");

            if (request.Host == "imgn1.magentozh.com")
                request.Referer = "http://www.verydm.com/";
            else if(referUrl.Contains("-") == false)
                request.Referer = referUrl;
           
            request.Headers.Add("Cache-Control:max-age=0");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
            request.Timeout = 10000;

            try
            {
                HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
                imgstream = resp.GetResponseStream();

                switch (resp.ContentEncoding)
                {
                    case "gzip": imgstream = DecompressEncode.DecompressGzipStream(imgstream); break;
                    case "deflate": ; break;
                }

                return imgstream;
            }
            catch(Exception ex)
            {
                Console.WriteLine("-----下载超时------");
                return imgstream;
            }
            
        }

        public int GetDownLoadSpeed()//获取下载速度
        {
            var speed = 0;
            return speed;
        }
    
    }

    public class DownLoadFile
    {
        public string SavePath { get; set; }//下载位置
        public string ComicName { get; set; }
        public string ComicUrl { get; set; }
    }
}
