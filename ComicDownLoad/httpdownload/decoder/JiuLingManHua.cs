using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace comicDownLoad
{
    class JiuLingManHua:PublicThing
    {
       // public string hostAttach = "http://www.90mh.com";
        public string hostAttach = "http://m.90mh.com";
    
        public override Queue<BasicComicInfo> GetTopComic(string response)
        {
            HtmlDocument doc;
            BasicComicInfo comicInfo;
            Queue<BasicComicInfo> queue;
            doc = new HtmlDocument();
            queue = new Queue<BasicComicInfo>();
            doc.LoadHtml(response);

            HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//div[@class='itemBox']/div[@class='itemImg']");
            
            if (collection == null)
            {
                return queue;
            }

            foreach (HtmlNode node in collection)
            {
                comicInfo = new BasicComicInfo();
                comicInfo.ComicHref = node.SelectSingleNode("./a").Attributes["href"].Value;
                comicInfo.ComicName = node.SelectSingleNode("./a/mip-img").Attributes["alt"].Value;
                comicInfo.ComicImgUrl = node.SelectSingleNode("./a/mip-img").Attributes["src"].Value;
                queue.Enqueue(comicInfo);
            }

            return queue;
        }

        public override ComicInfo GetComicInfo(string response)
        {
            HtmlDocument doc;
            ComicInfo info;
            doc = new HtmlDocument();
            doc.LoadHtml(response);
            info = new ComicInfo();

            Regex regex = new Regex(@"pageImage\s*=\s*""(?<url>[\w\.\:/-]*)");
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@class='comic-view clearfix']");
            HtmlNodeCollection collect = node.SelectNodes("./div/div/dl[@class='pic_zi fs15']");

            if (collect != null)
            {
                if (collect[4] != null && collect[4].SelectSingleNode("./dd/a") != null)
                {
                    info.Author = collect[4].SelectSingleNode("./dd/a").InnerText;
                    info.otherWork = collect[4].SelectSingleNode("./dd/a").Attributes["href"].Value;
                }
                else
                {
                    info.Author = "无";
                }

                if (collect[2].SelectSingleNode("./dd") != null)
                    info.HasFinished = collect[2].SelectSingleNode("./dd").InnerText;
                else
                    info.HasFinished = "无";

                if (collect[3].SelectNodes("./dd/a") != null)
                    info.Tag = collect[3].SelectNodes("./dd/a")[0].InnerText;
                else
                    info.Tag = "无";

                if(node.SelectSingleNode("//p[@class='txtDesc autoHeight']") != null)
                  info.Description = node.SelectSingleNode("//p[@class='txtDesc autoHeight']").InnerText.Replace("简介：", "") ;
            }

            
            Dictionary<string, string> dict = new Dictionary<string, string>();
            collect = doc.DocumentNode.SelectNodes("//ul[@class='Drama autoHeight']/li");
            
            if (collect == null)
            {
                info.URLDictionary = dict;
                return info;
            }

            string name = "";

            foreach (HtmlNode nodeTemp in collect)
            {
                node = nodeTemp.SelectSingleNode("./a");               
                name = node.SelectSingleNode("./span").InnerText;

                if (dict.ContainsKey(name) == false)
                { 
                    dict.Add(name, node.Attributes["href"].Value);
                }
            }

            info.URLDictionary = dict;
            return info;
        }
    
        public override CategoryCollect FindComicByCategory(string cateGoryStr)
        {
            HtmlDocument doc;
            BasicComicInfo comicInfo;
            Queue<BasicComicInfo> queue;
            doc = new HtmlDocument();
            CategoryCollect collect = new CategoryCollect();
            queue = new Queue<BasicComicInfo>();
            doc.LoadHtml(cateGoryStr);
            Dictionary<string, string> dict;
            dict = new Dictionary<string,string>();

            HtmlNodeCollection nodeCollection = doc.DocumentNode.SelectNodes("//li[@class='list-comic']");

            if (nodeCollection != null)
            {
                foreach (HtmlNode node in nodeCollection)
                {
                    comicInfo = new BasicComicInfo();
                    comicInfo.ComicHref = node.SelectSingleNode("./a").Attributes["href"].Value;
                    comicInfo.ComicName = node.SelectSingleNode("./a/mip-img").Attributes["alt"].Value.Replace("'", "");
                    comicInfo.ComicImgUrl = node.SelectSingleNode("./a/mip-img").Attributes["src"].Value;
                    queue.Enqueue(comicInfo);
                 }
            }

            string key = "";
            int current = 0;
            nodeCollection = doc.DocumentNode.SelectNodes("//ul[@class='pagination']/li");


            if (nodeCollection != null)
            {
                foreach (HtmlNode node in nodeCollection)
                {
                    if (node.SelectSingleNode("./a") == null)
                    {
                        continue;
                    }

                    key = node.SelectSingleNode("./a").InnerText;
                    
                    if (node.Attributes["class"] != null && node.Attributes["class"].Value == "active")
                    {
                        current = Convert.ToInt32(key);
                    }

                    if (dict.ContainsKey(key) == false)
                    {
                        dict.Add(key, node.SelectSingleNode("./a").Attributes["href"].Value);
                    }
                   
                }
              
            }
                       
            collect.ComicQueue = queue;
            collect.Count = queue.Count;
            collect.PagesCollection = dict;

            if(dict.ContainsKey((current+1).ToString()))
                collect.NextPageUrl = dict[(current + 1).ToString()];

            if (dict.ContainsKey((current - 1).ToString()))
                collect.LastPageUrl = dict[(current - 1).ToString()];
            return collect;
        }

        public override SearchResult GetSearchComic(string response)
        {
            SearchResult result = new SearchResult();
            BasicComicInfo comicInfo;
            HtmlDocument doc;
            doc = new HtmlDocument();
            doc.LoadHtml(response);
            Queue<BasicComicInfo> queue;
            queue = new Queue<BasicComicInfo>();
            HtmlNodeCollection nodeCollection = doc.DocumentNode.SelectNodes("//div[@class='itemImg']");

            if (nodeCollection != null)
            {
                foreach (HtmlNode node in nodeCollection)
                {
                    comicInfo = new BasicComicInfo();
                    comicInfo.ComicHref = node.SelectSingleNode("./a").Attributes["href"].Value;
                    comicInfo.ComicName = node.SelectSingleNode("./a/mip-img").Attributes["alt"].Value;
                    comicInfo.ComicImgUrl = node.SelectSingleNode("./a/mip-img").Attributes["src"].Value;
                    queue.Enqueue(comicInfo);
                }
            }

            HtmlNode nodeTemp = doc.DocumentNode.SelectSingleNode("//div[@class='Sub_H2 classify']");
           
            if (nodeTemp != null)
            {
                Regex regex = new Regex(@"共(?<num>\d+)");//这个有问题，无法匹配，而且没有给出下一页

                string num = regex.Match(nodeTemp.OuterHtml).Groups["num"].Value;
                result.Count = Convert.ToInt32(num);
            }

            int pos = 0;
            nodeCollection = doc.DocumentNode.SelectNodes("//ul[@class='pagination']/li");
            Dictionary<string, string> dict = new Dictionary<string, string>();
            
            if (nodeCollection != null)
            {

                foreach (var node in nodeCollection)
                {
                    if (node.Attributes["class"] != null && node.Attributes["class"].Value == "active")
                    {
                        pos = Convert.ToInt32(node.SelectSingleNode("./a").Attributes["data-page"].Value);
                    }

                    HtmlNode temp = node.SelectSingleNode("./a");

                    if (temp != null)
                    {
                        string key = temp.Attributes["data-page"].Value;
                        if (dict.ContainsKey(key) == false)
                        {
                            dict.Add(key, temp.Attributes["href"].Value);
                        }
                    }
                }

                if (dict.ContainsKey((pos + 1).ToString()))
                {
                    result.NextPageUrl = dict[(pos + 1).ToString()];
                }

                if (dict.ContainsKey((pos - 1).ToString()))
                {
                    result.LastPageUrl = dict[(pos - 1).ToString()];
                }
            }

            result.SearchQueue = queue;
            return result;
        }

        public override CategoryInfo GiveCategoryInfo(string response)
        {
            HtmlDocument doc;
            CategoryInfo info;
            info = new CategoryInfo();
            doc = new HtmlDocument();
            doc.LoadHtml(response);
            Dictionary<string, string> dict;
            dict = new Dictionary<string, string>();

            HtmlNodeCollection collect = doc.DocumentNode.SelectNodes("//div[@class='filter-item clearfix']/ul/li/a");
            
            foreach (HtmlNode node in collect)
            {
                if(dict.ContainsKey(node.InnerText) == false)
                    dict.Add(node.InnerText, node.Attributes["href"].Value);
            }
            info.ComicList = dict;
            return info;
        }

        private string GetDomain()
        {
            string serverUrl = "";
            string configUrl = "http://www.90mh.com/js/config.js";
            string response = AnalyseTool.HttpGet(configUrl);

            if (response == null)
            {
                serverUrl = "http://img.zzszs.com.cn";
            }

            string temp = response.Substring(response.IndexOf("domain"));
            temp = temp.Substring(temp.IndexOf("[") + 1, temp.IndexOf("]") - temp.IndexOf("[") - 1);
            serverUrl = temp.Replace("\"", "");
            return serverUrl;
        }

        public override DownLoadComic GetDownImageList(string response)//遍历，需要优化
        {
            int total = 0;
            HtmlDocument doc = new HtmlDocument();
            DownLoadComic comic = new DownLoadComic();

            if (response == "")
                return comic;

            doc.LoadHtml(response);
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//span[@id='k_total']");
            HtmlNodeCollection collect = doc.DocumentNode.SelectNodes("//div[@class='UnderPage']/div");

            string html = "";
            string htmlData = "";
            string nextPage = "";
            
            string baseHtml = collect[2].SelectSingleNode("./mip-link").Attributes["href"].Value;

            if (baseHtml.Contains("-"))
                html = baseHtml.Substring(0, baseHtml.LastIndexOf("-"));
            else
                html = baseHtml;
      
            total = Convert.ToInt32(node.InnerText);
            List<string> list = new List<string>();
            string imgUrl = collect[2].SelectSingleNode("./mip-link/mip-img").Attributes["src"].Value;
            list.Add(imgUrl);

            DateTime time = DateTime.Now;
          
            List<string> urlList = new List<string>();

            for (int i = 2; i < total + 1; i++)
            {
                nextPage = html + "-" + i + ".html";
                urlList.Add(nextPage);
            }

           string[] responseArry = AnalyseTool.HttpGet(urlList.ToArray());
           Console.WriteLine("获取所有访问耗时ms:{0}", DateTime.Now.Subtract(time).TotalMilliseconds);
           
           foreach(var i in responseArry)
           {
                if(i == "")
                {
                    return comic;
                }
               doc.LoadHtml(i);
               list.Add(doc.DocumentNode.SelectSingleNode("//mip-img").Attributes["src"].Value);
           }

           comic.ImageList = list;
           comic.Count = list.Count;
           return comic;
        }

    }
}
