using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace comicDownLoad
{
    public partial class DownLoadForm : Form
    {
        int panelPos = 0;
        DateTime time;
        NotifyIcon notify;
        List<DownListBox> downlistCollection = null;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public DownLoadForm()
        {
            InitializeComponent();
        }

        public List<string> ComicTitle { get; set; }

        private string GetWebRequest(string url, Encoding encode)
        {
            Uri uri = new Uri(url);
            WebRequest myReq = WebRequest.Create(uri);
            try
            {
                WebResponse result = myReq.GetResponse();
                Stream receviceStream = result.GetResponseStream();
                StreamReader readerOfStream = new StreamReader(receviceStream, encode);
                string strHTML = readerOfStream.ReadToEnd();
                readerOfStream.Close();
                receviceStream.Close();
                result.Close();
                return strHTML;
            }
            catch (Exception ex)
            {
                return "error" + ex.Message;
            }

        }

        public string HttpGet(string Url)//Http Get方法
        {
            Encoding encode;
            string retString = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "GET";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.Headers.Add("Accept-Encoding:gzip, deflate");//启用压缩编码
            request.Headers.Add("Cache-Control:max-age=0");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";
            request.KeepAlive = true;
            request.Host = new Uri(Url).Host;
            encode = Encoding.UTF8;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            switch (response.CharacterSet)
            {
                case "ISO-8859-1": encode = Encoding.GetEncoding("gb2312"); break;
                default: break;
            }
            Stream myResponseStream = response.GetResponseStream();

            switch (response.ContentEncoding)
            {
                case "gzip": retString = DecompressEncode.DecompressGzip(myResponseStream, encode); break;
                case "deflate": retString = DecompressEncode.DecompressDeflate(myResponseStream, encode); break;
                default: retString = DecompressEncode.NoCompress(myResponseStream, encode); break;
            }

            return retString;
        }

        private void DownOneFinsihed(object sender, DownLoadArgs args)
        {
            double fileLen = args.FileSize / 1024;
            double useTime = DateTime.Now.Subtract(time).Milliseconds/1000;
            if (useTime <= 0)
                useTime = 1;
            double netSpeed = fileLen / useTime;

            this.Invoke(new Action(() =>
            {
                //计算瞬时网速
                this.Text = "瞬时速度:" + netSpeed.ToString() + "K/S";

                foreach (var i in downlistCollection)
                {
                    if (args.TaskName == i.Title)
                    {
                        i.ProgressVal++;
                    }
                }
            }));
            
        }

        private void ShowMessage(string caption)//展示通知信息
        {
            notify = new NotifyIcon();
            notify.Icon = new Icon("laba.ico");
            notify.Click += notify_Click;
            notify.Visible = true;
            notify.ShowBalloonTip(1000, "通知", caption, ToolTipIcon.Info);
        }

        void notify_Click(object sender, EventArgs e)
        {
            MessageBox.Show("你点击了");
        }

        private void DownAllFinished(object sender, DownLoadArgs args)
        {
            string caption = args.TaskName + "下载完成";
            ShowMessage(caption);
        }

        private void DownPause(object sender, DownLoadArgs args)
        {
            string caption = args.TaskName + "下载出错";
            ShowMessage(caption);
        }

        private List<ResumeTask> CheckIfAllDownFinished(List<DownLoadFile> downloadFile)//判断是否存在该任务
        {
            SqlOperate operate = new SqlOperate();
            operate.CreateOrOpenDataBase("task.db");
            return operate.CheckDownFinished("task");
        }
      
        private void StartNewDownLoad(List<DownLoadFile> downloadFile, PublicThing decoder)//开始全新下载
        {
            var fullPath = "";           
            downlistCollection = new List<DownListBox>();
          
            try
            {                
                Task task = new Task(() =>
                {
                    DownListBox downlist = null;

                    foreach (var i in downloadFile)
                    {
                        DownTask downTask = new DownTask();
                        var url = i.ComicUrl;
                        var response = AnalyseTool.HttpGet(url);
                        var down = decoder.GetDownImageList(response);//解析图片真实地址

                        if (filePanel.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                downlist = new DownListBox();
                                downTask.ReferUrl = url;
                                downlist.SetMaxPage(down.ImageList.Count);//下载最大值
                                downlist.Title = i.ComicName;//漫画名字
                                downlist.Location = new Point(0, panelPos);
                                downlist.Pages = down.ImageList.Count;
                                filePanel.Controls.Add(downlist);
                            }));
                        }

                        downlistCollection.Add(downlist);//添加到控件集合
                        downTask.ComicName = i.ComicName;
                        downTask.downLoadOneFinished += DownOneFinsihed;//下载完成一个图片
                        downTask.downFinished += DownAllFinished;
                        downTask.downPaused += DownPause;
                        downTask.ImageFiles = down.ImageList;

                        if (File.Exists(i.SavePath) == false)
                        {
                            fullPath = i.SavePath + i.ComicName + "\\";
                            Directory.CreateDirectory(i.SavePath + i.ComicName + "\\");
                        }
                        
                        downlist.FilePath = fullPath;
                        downTask.DownLoadPath = fullPath;
                        time = DateTime.Now;
                        downTask.DownLoadStart();
                        panelPos = panelPos + downlist.Height;
                    }

                });

                task.Start();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("开始下载出错:{0}", ex.Message);
            }
        }

        private void ResumeDownLoadTask(List<ResumeTask> resumeTask)//断点下载
        {
            foreach (var i in resumeTask)//
            { 
                
            }
        }

        private void StopDownLoad()//存在该任务，且已经下载完成
        { 
            
        }

        public void Start(List<DownLoadFile> downloadFile, PublicThing decoder)//批量下载任务没有实现
        {      
            SqlOperate sqlOperate = new SqlOperate();
            sqlOperate.CreateOrOpenDataBase("task.db");

            try
            {
                var resumeTask = CheckIfAllDownFinished(downloadFile);

                switch (resumeTask.Count)
                {
                    case 0: StartNewDownLoad(downloadFile, decoder);break;//开启新的下载任务
                    default: ResumeDownLoadTask(resumeTask); break;//存在该任务，但没有下载完成，继续下载
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void newDownBtn_Click(object sender, EventArgs e)//新建下载
        {

        }

        private void DownConfigBtn_Click(object sender, EventArgs e)
        {
          
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();

            if (folder.ShowDialog() == DialogResult.OK)
            {
                savePathTexb.Text = folder.SelectedPath;
                ComicConfig.SaveConfig("downPath", savePathTexb.Text);
            }
        }

        private void DownLoadForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

    }


}
