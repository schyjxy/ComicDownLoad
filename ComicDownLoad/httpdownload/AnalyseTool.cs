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
using RestSharp;

namespace comicDownLoad
{
    class AnalyseTool
    {
        public static string HttpGet(string url)
        {
            RestClient client = new RestClient(url);
            client.UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Mobile Safari/537.36";
            client.BaseHost = new Uri(url).Host;
            RestRequest request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = url;
            request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");

            request.AddHeader("Cache-Control", "max-age=0");
            request.AddHeader("Referer", url);
            //request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.Timeout = 5000;
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content;
            }
            else
            {
                Console.WriteLine("网路错误");
            }

            return "";
        }


        //public static string HttpGet(string url)
        //{
        //    string retString = "";
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //    request.Method = "GET";
        //    request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
        //    request.Headers.Add("Cache-Control:max-age=0");
        //    request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
        //    request.KeepAlive = false;
        //    request.Timeout = 5000;
        //    request.Host = new Uri(url).Host;
        //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        //    Stream myResponseStream = response.GetResponseStream();
        //    StreamReader reader = new StreamReader(myResponseStream);
        //    retString = reader.ReadToEnd();
        //    reader.Close();
        //    myResponseStream.Close();
        //    return retString;
        //}

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

        public static Image GetImage(string url)
        {
            RestClient client = new RestClient();
            client.UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Mobile Safari/537.36";
            client.BaseHost = new Uri(url).Host;
            RestRequest request = new RestRequest();
            request.Method = Method.GET;
            request.Resource = url;
            request.AddHeader("Accept", "image/webp,image/*,*/*;q=0.8");
            request.AddHeader("Cache-Control", "max-age=0");
            request.AddHeader("Accept-Encoding", "gzip, deflate");

            if (client.BaseHost == "imgn1.magentozh.com")
            {
                request.AddHeader("Referer", "http://www.verydm.com/");
            }
            else
            {
                //request.AddHeader("Referer", url);
            }
           
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                MemoryStream memory = new MemoryStream();
                memory.Write(response.RawBytes, 0, response.RawBytes.Length);
                Image image =  Image.FromStream(memory);//Image没有释放
                memory.Close();
                return image;
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
