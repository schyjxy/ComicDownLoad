using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace comicDownLoad
{
    class VeryDm:PublicThing
    {
        string hostName = "http://www.verydm.com";

        string[] arryData = new string[]
         {
             "0", "1", "2" ,"3", "4", "5",
             "6", "7", "8", "9", "a", "b",
             "c", "d", "e", "f", "g", "h",
             "i", "j", "k", "l", "m", "n",
             "o", "p", "q", "r", "s", "t",
             "u", "v", "w", "x", "y", "z"               
         };

        public string DecTo36(int val)
        {
            int temp1 = val;
            int temp = 0;
            string result = "";
            List<string> save;
            save = new List<string>();

            if (val < 36)
            {
                return arryData[val];
            }

            while (temp1 > 0)
            {
                temp = temp1 % 36;
                save.Add(arryData[temp]);
                temp1 = temp1 / 36;
            }

            for (int i = save.Count - 1; i >= 0; i--)
            {
                result = result + save[i];
            }

            return result;
        }

        public string eFun(int c, int a)
        {
            return (c < a ? "" : eFun(c / a, a)) + ((c = c % a) > 35 ? Convert.ToChar(c + 29).ToString() : DecTo36(c));
        }

        public string DecodeEncryUrl(string p, int a, int c, string[] k)
        {
            string replaceText = "";

            while (c--> 0)
            {
                if (k[c] != "")
                {
                    replaceText = @"\b" + eFun(c, a) + @"\b";
                    p = Regex.Replace(p, replaceText, k[c]);
                    Console.WriteLine("替换表达式:{0},替换值:{1}", replaceText, k[c]);
                }
            }

            return p;
        }

        public List<string> AnalyseData(string data)
        {
            var a = 0;
            var c = 0;
            var urlData = "";
            List<string> urlList;
            MatchCollection collect;
            string[] dicArry;
            Regex[] regexArry;
            Regex regex = new Regex(@"\}\((?<data>.+)\.");
            
            regexArry = new Regex[3];
            regexArry[1] = new Regex(@"(?<data>\d+)\,");
            
            var temp = regex.Match(data).Groups["data"].Value;
            collect = regexArry[1].Matches(temp);
            a = Convert.ToInt32(collect[0].Groups["data"].Value);
            c = Convert.ToInt32(collect[1].Groups["data"].Value);
            regexArry[2] = new Regex(c.ToString() + @"\,.+");
            var dic = temp.Substring(temp.IndexOf(c.ToString() + ",'")+4);
            dic = dic.Substring(0, dic.LastIndexOf("'"));
            dicArry = dic.Split('|');

            temp = temp.Substring(temp.IndexOf("[")+1, temp.LastIndexOf("]") - temp.IndexOf("[")-1);
            urlData = DecodeEncryUrl(temp, a, c, dicArry);
            string[] urlArry = urlData.Split(',');
            urlList = new List<string>();

            foreach (var i in urlArry)
            {
                temp = i.Replace(@"""", "").Replace(@"\","");
                urlList.Add(temp);
            }

            return urlList;
        }

        public override Queue<BasicComicInfo> GetTopComic(string response)
        {
            HtmlDocument doc;
            Queue<BasicComicInfo> queue;
            BasicComicInfo basicInfo;

            doc = new HtmlDocument();
            queue = new Queue<BasicComicInfo>();
            doc.LoadHtml(response);
            HtmlNodeCollection collect = doc.DocumentNode.SelectNodes("//a[@class='cover']");

            foreach (HtmlNode tempNode in collect)
            {
                basicInfo = new BasicComicInfo();
                basicInfo.ComicHref = tempNode.Attributes["href"].Value;
                basicInfo.ComicName = tempNode.Attributes["title"].Value;
                basicInfo.ComicImgUrl = tempNode.SelectSingleNode("./img").Attributes["src"].Value;
                queue.Enqueue(basicInfo);
            }

            return queue;
        }

        public override ComicInfo GetComicInfo(string response)
        {
            var url = "";
            ComicInfo info = new ComicInfo();
            Dictionary<string, string> dict;
            dict = new Dictionary<string, string>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            HtmlNodeCollection collect = doc.DocumentNode.SelectNodes("//ul[@class='detail']/li");
            info.Author = collect[0].SelectSingleNode("./a").InnerText;
            info.otherWork = collect[0].SelectSingleNode("./a").Attributes["href"].Value;
            info.HasFinished = collect[2].InnerText.Replace("状态：", "");
            info.Tag = collect[3].InnerText.Replace("分类：","");

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@id='summary']");
            info.Description = doc.DocumentNode.SelectSingleNode("//div[@id='content_wrapper']").InnerText;
            node = doc.DocumentNode.SelectSingleNode("//div[@class='chapters']/ul");
            collect = node.SelectNodes("./li/a");
            
            foreach (HtmlNode temp in collect)
            {
                url = hostName + temp.Attributes["href"].Value;

                if (dict.ContainsKey(temp.InnerText) == false)
                {
                    dict.Add(temp.InnerText, url);
                }
            }

            info.URLDictionary = dict;
            return info;
        }

        public override DownLoadComic GetDownImageList(string response)
        {
            var temp = "";
            DownLoadComic comic = new DownLoadComic();
            HtmlDocument doc = new HtmlDocument();

            try
            {                             
                doc.LoadHtml(response);
                temp = doc.DocumentNode.SelectSingleNode("//script[@type='text/javascript']").InnerText.Trim();
                comic.ImageList = AnalyseData(response);
                comic.Count = comic.ImageList.Count;
                foreach (var i in comic.ImageList)
                {
                    Console.WriteLine("网址:{0}", i);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("非常动漫DownLoad函数失败");
            }
            return comic;
        }

        public override CategoryInfo GiveCategoryInfo(string response)
        {
            CategoryInfo info = new CategoryInfo();
            Dictionary<string, string> dict;
            dict = new Dictionary<string, string>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            HtmlNodeCollection collect = doc.DocumentNode.SelectNodes("//ul[@class='clearfix category']/li/a");
            
            foreach (HtmlNode node in collect)
            {
                if (dict.ContainsKey(node.InnerText) == false)
                {
                    dict.Add(node.InnerText, hostName + node.Attributes["href"].Value);
                }

            }

            info.ComicList = dict;
            return info;
        }

        public override CategoryCollect FindComicByCategory(string response)
        {
            HtmlDocument doc;
            CategoryCollect cateInfo;
            Queue<BasicComicInfo> queue;
            Dictionary<string,string> pageDict;

            doc = new HtmlDocument();
            cateInfo = new CategoryCollect();          
            queue = new Queue<BasicComicInfo>();
            doc.LoadHtml(response);
            BasicComicInfo basicInfo = null;
            HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//ul[@class='grid-row clearfix']/li | //ul[@class='grid-row clearfix first']/li");
            
            foreach (HtmlNode node in collection)
            {
                basicInfo = new BasicComicInfo();
                basicInfo.ComicName = node.SelectSingleNode("./p/a").InnerText;
                basicInfo.ComicHref = node.SelectSingleNode("./p/a").Attributes["href"].Value;
                basicInfo.ComicImgUrl = node.SelectSingleNode("./a/img").Attributes["src"].Value;
                queue.Enqueue(basicInfo);
            }

            pageDict = new Dictionary<string,string>();
            collection = doc.DocumentNode.SelectNodes("//ul[@class='pagination']/li/a");
            
            foreach(HtmlNode node in collection)
            {
              if(pageDict.ContainsKey(node.InnerText) == false)
              {
                pageDict.Add(node.InnerText,hostName + node.Attributes["href"].Value);
              }
            }

            var temp = doc.DocumentNode.SelectSingleNode("//div[@class='head']/span").InnerText;
            var currentPage = Convert.ToInt32(temp.Substring(0, temp.IndexOf("/")));
            cateInfo.ComicQueue = queue;
            cateInfo.Count = queue.Count;
            cateInfo.NextPageUrl = pageDict[(currentPage+1).ToString()];
            cateInfo.PagesCollection = pageDict;
            return cateInfo;
        }
    }
}
