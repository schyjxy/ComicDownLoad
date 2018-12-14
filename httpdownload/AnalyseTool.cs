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

namespace comicDownLoad
{

    public class TwoTreeNode
    {
        public string data;
        public TwoTreeNode leftChild;
        public TwoTreeNode rightChild;
    }

    class AnalyseTool
    {
        public static string HttpGet(string Url)//Http Get方法
        {
            Encoding encode;
            string retString = "";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);

                if (Url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11;
                    ServicePointManager.CheckCertificateRevocationList = true;
                    ServicePointManager.DefaultConnectionLimit = 100;
                    ServicePointManager.Expect100Continue = false;
                }

                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Headers.Add("Accept-Encoding:gzip, deflate");//启用压缩编码
                request.Headers.Add("Cache-Control:max-age=0");
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";
                request.KeepAlive = false;
                request.Host = new Uri(Url).Host;
                request.Timeout = 5000;

                encode = Encoding.UTF8;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                switch (response.CharacterSet)
                {
                    case "ISO-8859-1":
                        if (request.Host != "www.dmzx.com")
                        {
                            encode = Encoding.UTF8;
                        }
                        else
                        {
                            encode = Encoding.GetEncoding("gb2312");
                        }
                        break;
                    case "gb2312": encode = Encoding.GetEncoding("gb2312"); break;
                    default: break;
                }

                Stream myResponseStream = response.GetResponseStream();

                switch (response.ContentEncoding)
                {
                    case "gzip": retString = DecompressEncode.DecompressGzip(myResponseStream, encode); break;
                    case "deflate": retString = DecompressEncode.DecompressDeflate(myResponseStream, encode); break;
                    default: retString = DecompressEncode.NoCompress(myResponseStream, encode); break;
                }

            }
            catch (Exception ex)
            {
                retString = "";
            }
            return retString;
        }

        public static string HttpGet(string Url, string referUrl)//Http Get方法
        {
            Encoding encode;
            string retString = "";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);

                if (Url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11;
                    ServicePointManager.CheckCertificateRevocationList = true;
                    ServicePointManager.DefaultConnectionLimit = 100;
                    ServicePointManager.Expect100Continue = false;
                }

                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Headers.Add("Accept-Encoding:gzip, deflate");//启用压缩编码
                request.Headers.Add("Cache-Control:max-age=0");
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";
                request.KeepAlive = false;
                request.Referer = referUrl;
                request.Host = new Uri(Url).Host;
                request.Timeout = 5000;

                encode = Encoding.UTF8;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                switch (response.CharacterSet)
                {
                    case "ISO-8859-1":
                        if (request.Host != "www.dmzx.com")
                        {
                            encode = Encoding.UTF8;
                        }
                        else
                        {
                            encode = Encoding.GetEncoding("gb2312");
                        }
                        break;
                    case "gb2312": encode = Encoding.GetEncoding("gb2312"); break;
                    default: break;
                }

                Stream myResponseStream = response.GetResponseStream();

                switch (response.ContentEncoding)
                {
                    case "gzip": retString = DecompressEncode.DecompressGzip(myResponseStream, encode); break;
                    case "deflate": retString = DecompressEncode.DecompressDeflate(myResponseStream, encode); break;
                    default: retString = DecompressEncode.NoCompress(myResponseStream, encode); break;
                }

            }
            catch (Exception ex)
            {
                retString = "";
            }
            return retString;
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

        public static string HttpsGet(string url)
        {
            string ret = "";
            //url = "https://www.douban.com/";
            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11;
            ServicePointManager.CheckCertificateRevocationList = true;
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.Expect100Continue = false;
                      
            request.Method = "GET";
            request.ProtocolVersion = HttpVersion.Version11;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            request.Headers.Add("Accept-Encoding:gzip, deflate");//启用压缩编码
            request.Headers.Add("Cache-Control:max-age=0");;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
            request.Host = new Uri(url).Host;
                    
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader read = new StreamReader(myResponseStream, Encoding.UTF8);
            ret = read.ReadToEnd();
            return ret;
        }

        public static string PostWebRequest(string postUrl, string paramData)
        {
            string ret = string.Empty;
            try
            {
                if (!postUrl.StartsWith("http://"))
                    return "";

                byte[] byteArray = Encoding.Default.GetBytes(paramData); //转化
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                webReq.ContentType = "application/x-www-form-urlencoded";

                webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);//写入参数
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
                newStream.Close();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return ret;
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
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

        public static System.Drawing.Image GetImage(string url)
        {
            try
            {
                HttpWebRequest myrequest = WebRequest.Create(url) as HttpWebRequest;
                myrequest.Accept = "image/webp,image/*,*/*;q=0.8";
                myrequest.KeepAlive = true;
                myrequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
                myrequest.Method = "GET";
                myrequest.Host = new Uri(url).Host;
               
                if (myrequest.Host == "imgn1.magentozh.com")
                    myrequest.Referer = "http://www.verydm.com/";

                HttpWebResponse resp = (HttpWebResponse)myrequest.GetResponse();

                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    Stream imgstream = resp.GetResponseStream();
                    return System.Drawing.Image.FromStream(imgstream);
                }
                else
                {
                    return null;
                }
            }
            catch (WebException ex)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public static void CreateTree()
        {
            string testStr = "ABDEACF#";
            TwoTreeNode node = null;
            List<TwoTreeNode> list = new List<TwoTreeNode>();
 
            for (int i = 0; i < testStr.Length; i++)
            {
                if (testStr[i] != '#')
                {
                    node = new TwoTreeNode();

                }
                else
                    node = null;
            }
            
        }

        public static void GetTagTree()//二叉树实现属性获取
        { 
            
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

        private static bool CompareStr(string sourceStr, int index, string compareStr)
        {
            bool isSame = true;
            for (int i = 0; i < compareStr.Length; i++)
            {
                if (sourceStr[i + index] != compareStr[i])
                {
                    isSame = false;
                    break;
                }
            }
            return isSame;
        }

    }
}
