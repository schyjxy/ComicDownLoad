using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Security;
using System.IO;
using System.IO.Compression;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Drawing;
using System.Threading;
using Microsoft.JScript;
using Microsoft.JScript.Vsa;

namespace comicDownLoad
{
    class AnalyseTool
    {
        static int imageCount = 0;
        static Image[] imageArry;
        static string[] responseArry;
        static int urlCount = 0;
        static object objLock = new object();
        static AutoResetEvent getUrlEvent = new AutoResetEvent(false);
        static AutoResetEvent getImageEvent = new AutoResetEvent(false);

        class ImageInfo
        {
            public int count;
            public string url;
        }

        class UrlInfo
        {
            public int count;
            public string url;
        }

        public static string HttpGet(string Url, string refer = null)//Http Get方法
        {
            string retString = "";
            try
            {
                DateTime time = DateTime.Now;
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Method = "GET";
                //request.Accept = "*/*";
                request.UseDefaultCredentials = true;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Host = new Uri(Url).Host;
                request.Headers.Add("Accept-Encoding:gzip, deflate");//启用压缩编码
                request.Headers.Add("Cache-Control:max-age=0");
                request.ServicePoint.Expect100Continue = false;
                request.Timeout = 3000;

                if(refer != null)
                    request.Referer = refer;

                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
                request.KeepAlive = true;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();

                switch (response.ContentEncoding)
                {
                    case "gzip": retString = DecompressEncode.DecompressGzip(myResponseStream, Encoding.UTF8); break;
                    case "deflate": retString = DecompressEncode.DecompressDeflate(myResponseStream, Encoding.UTF8); break;
                    default: retString = DecompressEncode.NoCompress(myResponseStream, Encoding.UTF8); break;
                }
            }
            catch(Exception ex)
            {
                return retString;
            }

            //Console.WriteLine("Get请求耗时:{0} ms", DateTime.Now.Subtract(time).Milliseconds);
            return retString;
        }

        private static void GetHttp(object obj)
        {
            UrlInfo info = obj as UrlInfo;
            string reponse = HttpGet(info.url);

            lock (objLock)
            {
                if (reponse == "")
                {
                    reponse = HttpGet(info.url);
                }

                responseArry[info.count] = reponse;
                urlCount--;

                if (urlCount == 0)
                {
                    getUrlEvent.Set();
                }
            }            
           
        }

        public static ArrayObject EvalJScript(string JScript)
        {
            VsaEngine Engine = VsaEngine.CreateEngine();

            ArrayObject Result = null;
            try
            {
                Result = Microsoft.JScript.Eval.JScriptEvaluate(JScript, Engine) as ArrayObject;
            }
            catch (Exception ex)
            {
                return null;
            }
            return Result;
        }

        public static string[] HttpGet(string[] urlArry)
        {
            responseArry = new string[urlArry.Length];
            ThreadPool.SetMinThreads(50, 50);
            ThreadPool.SetMaxThreads(100, 100);
            urlCount = urlArry.Length;
            UrlInfo info;

            for (int i=0;i<urlArry.Length;i++)
            {
                info = new UrlInfo();
                info.count = i;
                info.url = urlArry[i];
                ThreadPool.QueueUserWorkItem(GetHttp, info);
            }

            getUrlEvent.WaitOne();
            return responseArry;
        }

        public static string HttpGet(string Url, Encoding encode)//Http Get方法
        {
            string retString = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "GET";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.Headers.Add("Accept-Encoding:gzip, deflate");//启用压缩编码
            request.Headers.Add("Cache-Control:max-age=0");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
            request.KeepAlive = true;
            request.Host = new Uri(Url).Host;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();

            switch (response.ContentEncoding)
            {
                case "gzip": retString = DecompressEncode.DecompressGzip(myResponseStream, encode); break;
                case "deflate": retString = DecompressEncode.DecompressDeflate(myResponseStream, encode); break;
                default: retString = DecompressEncode.NoCompress(myResponseStream, encode); break;
            }

            return retString;
        }

        public static string GetTag(string sourceStr, string startStr, string endStr)
        {
            var retStr = "";

            if (sourceStr != null)
            {
                retStr = sourceStr.Substring(sourceStr.IndexOf(startStr));
                retStr = retStr.Substring(0, retStr.IndexOf(endStr));
            }
            return retStr;
        }

        private static void DownImage(object obj)
        {
            ImageInfo info = obj as ImageInfo;
            Image img = GetImage(info.url);

            if(img == null)
            {
                Console.WriteLine("图像为空");
            }
            else
            {
                Console.WriteLine("图像下载成功");
            }

            imageArry[info.count] = img;
            imageCount--;

            if (imageCount == 0)
            {
                getImageEvent.Set();
            }
        }

        public static Image[] GetImage(string []url)//并发获取图片
        {
            imageCount = url.Length;
            ThreadPool.SetMinThreads(10, 10);
            ThreadPool.SetMinThreads(50, 50);

            if(imageArry != null)
            {
                Array.Clear(imageArry, 0, imageArry.Length);
            }

            ImageInfo info;
            imageArry = new Image[url.Length];

            for (int i = 0;i < url.Length;i++)
            {
                info = new ImageInfo();
                info.count = i;
                info.url = url[i];            
                ThreadPool.QueueUserWorkItem(DownImage, info);
            }
            
            getImageEvent.WaitOne();
            return imageArry;
        }    

        public static Image GetImage(string url)
        {
            DateTime time = DateTime.Now;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "image/webp,image/*,*/*;q=0.8";
            request.Headers.Add("Accept-Encoding:gzip, deflate");//启用压缩编码
            request.Headers.Add("Cache-Control:max-age=0");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
            request.Host = new Uri(url).Host;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    MemoryStream memory = new MemoryStream();
                    myResponseStream.CopyTo(memory);
                    Image image = Image.FromStream(memory);//Image没有释放
                    memory.Close();
                    //Console.WriteLine("Get获取图片耗时:{0} ms", DateTime.Now.Subtract(time).Milliseconds);
                    return image;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("GetImage函数错误:{0}", ex.Message);
            }

            return null;
        }

        public static string ReplacePunctuation(string sourceStr)
        {            
            sourceStr = sourceStr.Replace("&mdash;", "-");
            sourceStr = sourceStr.Replace(@"&middot;", "·");
            sourceStr = sourceStr.Replace("&ldquo;", "“");
            sourceStr = sourceStr.Replace("&rdquo;", "”");
            sourceStr = sourceStr.Replace("&hellip;", "……");
            sourceStr = sourceStr.Replace("<p>", "  ");
            sourceStr = sourceStr.Replace("&nbsp;", "");
            sourceStr = sourceStr.Replace("</p>", "");
            sourceStr = sourceStr.Replace("\r\n", "");
            sourceStr = sourceStr.Replace(" ", "");
            return sourceStr;
        }


    }
}
