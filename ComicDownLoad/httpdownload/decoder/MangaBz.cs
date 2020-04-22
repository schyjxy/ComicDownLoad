using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;

namespace comicDownLoad
{
    class MangaBz:PublicThing
    {
        const string host = "http://www.mangabz.com";

        public override Queue<BasicComicInfo> GetTopComic(string response)//实现热门
        {
            BasicComicInfo basicComicInfo;
            HtmlNode mainNode = GetMainNode(response);
            Queue<BasicComicInfo> queue = new Queue<BasicComicInfo>();
            HtmlNodeCollection nodes = mainNode.SelectNodes("//div[@class='manga-i-list-item']");

            if(nodes != null)
            {
                foreach(HtmlNode temp in nodes)
                {
                    basicComicInfo = new BasicComicInfo();
                    basicComicInfo.ComicHref = host + temp.SelectSingleNode("./a").Attributes["href"].Value;
                    basicComicInfo.ComicName = temp.SelectSingleNode("./a").Attributes["title"].Value;
                    basicComicInfo.ComicImgUrl = temp.SelectSingleNode("./a/img").Attributes["src"].Value;
                    queue.Enqueue(basicComicInfo);
                }
            }
            return queue;   
        }

        public override ComicInfo GetComicInfo(string response)
        {
            ComicInfo comicInfo = new ComicInfo();
            HtmlNode mainNode = GetMainNode(response);

            HtmlNode node = mainNode.SelectNodes("//span[@class='block']")[0];
            comicInfo.Author = node.SelectSingleNode("./a/span").InnerText;

            node = mainNode.SelectNodes("//span[@class='block']")[1];
            comicInfo.HasFinished = mainNode.SelectSingleNode("//span[@class='detail-list-left']").InnerText;
            comicInfo.Description = mainNode.SelectSingleNode("//p[@class='detail-main-content']").InnerText;
            comicInfo.URLDictionary = new Dictionary<string, string>();

            HtmlNodeCollection htmlNodes = mainNode.SelectNodes("//div[@class='detail-list']/div/a");

            if (htmlNodes != null)
            {
                foreach(HtmlNode temp in htmlNodes)
                {
                    string name = temp.InnerHtml.Trim();

                    if(!comicInfo.URLDictionary.ContainsKey(name))
                    {
                        comicInfo.URLDictionary.Add(name, host + temp.Attributes["href"].Value);
                    }
                }
            }
            return comicInfo;
        }

        public override CategoryCollect FindComicByCategory(string cateGoryStr)
        {
            CategoryCollect collect;
            BasicComicInfo basicComicInfo;
            collect = new CategoryCollect();
            HtmlNode mainNode = GetMainNode(cateGoryStr);

            Queue<BasicComicInfo> queue = new Queue<BasicComicInfo>();
            HtmlNodeCollection nodes = mainNode.SelectNodes("//div[@class='manga-i-list-item']");

            if (nodes != null)
            {
                foreach (HtmlNode temp in nodes)
                {
                    basicComicInfo = new BasicComicInfo();
                    basicComicInfo.ComicHref = host + temp.SelectSingleNode("./a").Attributes["href"].Value;
                    basicComicInfo.ComicName = temp.SelectSingleNode("./p").InnerText;
                    basicComicInfo.ComicImgUrl = temp.SelectSingleNode("./a/img").Attributes["src"].Value;
                    queue.Enqueue(basicComicInfo);
                }
            }

            int curIndex = 0;
            int count = 0;
            nodes = mainNode.SelectNodes("//div[@class='page-pagination']/ul/li");
            collect.PagesCollection = new Dictionary<string, string>();

            if(nodes != null)
            {
                foreach(HtmlNode temp in nodes)//这部分有问题，找不到page-pagination段
                {
                    if (!collect.PagesCollection.ContainsKey(temp.InnerText))
                    {
                        if(temp.Attributes["class"] != null)
                        {
                            curIndex = count;
                        }
                        collect.PagesCollection.Add(temp.InnerText, host + temp.Attributes["href"].Value);
                        count++;
                    }
                }
            }

            collect.ComicTotalCount = queue.Count;
            collect.ComicQueue = queue;
            return collect;
        }

        public override DownLoadComic GetDownImageList(string response)
        {
            DownLoadComic downLoad = new DownLoadComic();
            Regex urlRegex = new Regex(@"MANGABZ_CURL\s*=\s*""(?<data>[\w/]*)""");
            Regex midRegex = new Regex(@"MANGABZ_COMIC_MID=(?<data>\d+)");
            Regex cidRegex = new Regex(@"MANGABZ_CID=(?<data>\d+)");
            Regex signRegex = new Regex(@"MANGABZ_VIEWSIGN=""(?<data>\w+)""");
            Regex dtRegex = new Regex(@"MANGABZ_VIEWSIGN_DT=""(?<data>[\w\s\-\:]*)""");

            var url = urlRegex.Match(response).Groups["data"].Value;
            var mid = midRegex.Match(response).Groups["data"].Value;
            var cid = cidRegex.Match(response).Groups["data"].Value;
            var sign = signRegex.Match(response).Groups["data"].Value;
            var dt = dtRegex.Match(response).Groups["data"].Value;

            var requestUrl = host +  string.Format("{0}chapterimage.ashx?cid={1}&page={2}&key=&" +
                "_cid={1}&_mid={3}&_dt={4}&sign={5}",url, cid, 1, mid, System.Web.HttpUtility.UrlEncode(dt), sign);

            var msg = AnalyseTool.HttpGet(requestUrl);

            return downLoad;
        }

        public override CategoryInfo GiveCategoryInfo(string response)
        {
            string name = "";
            HtmlNode mainNode = GetMainNode(response);
            CategoryInfo info = new CategoryInfo();
            info.ComicList = new Dictionary<string, string>(0);
            HtmlNodeCollection htmlNodes = mainNode.SelectNodes("//div[@class='class-list']");

            if(htmlNodes != null)
            {
                foreach(HtmlNode node in htmlNodes)
                {
                    HtmlNodeCollection collection = node.SelectNodes("./a");

                    foreach(HtmlNode node1 in collection)
                    {
                        name = node1.InnerText;

                        if (!info.ComicList.ContainsKey(name))
                        {
                            info.ComicList.Add(name, host + node1.Attributes["href"].Value);
                        }
                    }
                    
                }
            }
            return info;
        }
    }
}
