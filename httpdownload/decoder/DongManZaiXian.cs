using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace comicDownLoad
{
    class DongManZaiXian:PublicThing
    {
        string hostName = "http://www.dmzx.com";

        public override Queue<BasicComicInfo> GetTopComic(string response)
        {
            HtmlNode node;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);          
            BasicComicInfo basic = null;
            Queue<BasicComicInfo> queue;
            queue = new Queue<BasicComicInfo>();
            HtmlNodeCollection collect = doc.DocumentNode.SelectNodes("//div[@class='picbox']");

            if (collect == null)
            {
                return queue;
            }

            foreach (HtmlNode temp in collect)
            {
                basic = new BasicComicInfo();
                node = temp.SelectSingleNode("./a");
                basic.ComicName = node.Attributes["title"].Value;
                basic.ComicHref = node.Attributes["href"].Value;
                node = node.SelectSingleNode("./img");

                if (node.Attributes["data-original"] == null)
                {
                    continue;
                }

                basic.ComicImgUrl = hostName + node.Attributes["data-original"].Value;
                queue.Enqueue(basic);
            }

            return queue;
        }

        public override ComicInfo GetComicInfo(string response)
        {
            HtmlNode node;
            ComicInfo comicInfo;
            comicInfo = new ComicInfo();
            HtmlDocument doc = new HtmlDocument();
            Dictionary<string, string> dict;
            doc.LoadHtml(response);
            dict = new Dictionary<string, string>();

            try
            {
                node = doc.DocumentNode.SelectSingleNode("//div[@class='sectioninfo allfloatleft gray']");
                HtmlNodeCollection collect = node.SelectNodes("./p");                 
                comicInfo.Author = collect[1].SelectSingleNode("./span/a").InnerText;
                comicInfo.HasFinished = AnalyseTool.ReplacePunctuation(collect[2].SelectSingleNode("./span/a").InnerText);
                comicInfo.Tag = collect[3].SelectSingleNode("./span/a").InnerText;
                node = doc.DocumentNode.SelectSingleNode("//div[@id='mhlist']");
               
                if (node == null)
                {
                    comicInfo.Description = "因版权、国家法规等原因，动漫在线暂停提供此漫画的在线观看,欢迎大家继续观看其他精彩漫画，此漫画恢复情况请关注动漫在线";
                    comicInfo.URLDictionary = new Dictionary<string, string>();
                    return comicInfo;
                }

                collect = node.SelectNodes("./ul/li/a");

                foreach (HtmlNode temp in collect)
                {
                    if (dict.ContainsKey(temp.Attributes["title"].Value) == false)
                        dict.Add(temp.Attributes["title"].Value, temp.Attributes["href"].Value);
                }

                node = doc.DocumentNode.SelectSingleNode("//div[@id='mh_smalltext']");
                comicInfo.Description = node.InnerText;
                comicInfo.URLDictionary = dict;
            }
            catch (Exception ex)
            {
                comicInfo.Description = "该漫画已下架";
                comicInfo.URLDictionary = new Dictionary<string, string>();
                return comicInfo;
            }
            return comicInfo;
        }

        public override DownLoadComic GetDownImageList(string response)
        {
            string host = "";
            List<string> comicList;
            DownLoadComic comic = new DownLoadComic();
            Regex regexUrl = new Regex(@"picAy\[\d+\]=""(?<url>[\w_%-.]*)""");
            Regex regexDomain = new Regex(@"picHosts\s*=\s*""(?<host>[\w\:/\.]*)""");
            host = regexDomain.Match(response).Groups["host"].Value;

            MatchCollection matchCollect = regexUrl.Matches(response);
            comicList = new List<string>();

            foreach (Match M in matchCollect)
            { 
                comicList.Add(host + M.Groups["url"].Value);
            }

            comic.ImageList = comicList;
            comic.Count = comicList.Count;
            return comic;
        }

        public override CategoryInfo GiveCategoryInfo(string response)
        {
            string href = "";
            CategoryInfo cateInfo = new CategoryInfo();
            HtmlDocument doc = new HtmlDocument();
            Dictionary<string, string> cateDict;
            doc.LoadHtml(response);
            cateDict = new Dictionary<string, string>();
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@class='submenu gray']");
            HtmlNodeCollection collect = node.SelectNodes("./ul/li/a");
            
            foreach (HtmlNode temp in collect)
            {
                href = temp.Attributes["href"].Value;

                if (cateDict.ContainsKey(temp.Attributes["title"].Value) == false && href != "#" && temp.Attributes["title"].Value!= "会员中心")
                    cateDict.Add(temp.Attributes["title"].Value, href);
            }

            cateInfo.ComicList = cateDict;
            return cateInfo;
        }

        public override CategoryCollect FindComicByCategory(string cateGoryStr)
        {
            HtmlDocument doc;
            HtmlNode node;
            BasicComicInfo basicInfo;
            HtmlNodeCollection collect;
            CategoryCollect cateCollect;
            Queue<BasicComicInfo> comicQueue;
            basicInfo = null;
            doc = new HtmlDocument();
            cateCollect = new CategoryCollect();
            comicQueue = new Queue<BasicComicInfo>();
            doc.LoadHtml(cateGoryStr);

            collect = doc.DocumentNode.SelectNodes("//div[@class='recommendedpic allfloatleft']");
            if (collect == null)
            {
                collect = doc.DocumentNode.SelectNodes("//div[@class='recommendedpicl center']");   
            }
            
            foreach(HtmlNode temp in collect)
            {
                basicInfo = new BasicComicInfo();
                node = temp.SelectSingleNode("./a");
                basicInfo.ComicName = node.Attributes["title"].Value;
                basicInfo.ComicHref = node.Attributes["href"].Value;
                basicInfo.ComicImgUrl = hostName +  temp.SelectSingleNode("./a/img").Attributes["src"].Value;
                comicQueue.Enqueue(basicInfo);
            }

            string href = "";
            HtmlNode hrefNode = doc.DocumentNode.SelectSingleNode("//div[@class='gray reminderguild']");

            Regex regex = new Regex(@"href=""(?<href>[\w_\.]*)""\s*title=""下一页""");
            Regex regexhref = new Regex(@"href=""(?<href>[\w\-\./]*)""\s*target=""_self"">全部");

            if (hrefNode != null)
            {
                href = regexhref.Match(hrefNode.OuterHtml).Groups["href"].Value;
                href = href.Substring(0, href.LastIndexOf("/")) + "/";
                cateCollect.NextPageUrl = hostName + href + regex.Match(cateGoryStr).Groups["href"].Value;
            }
            else
                cateCollect.NextPageUrl = "http://www.dmzx.com/zuixin/";

            cateCollect.Count = comicQueue.Count;
            cateCollect.ComicQueue = comicQueue;
            return cateCollect;
        }
    }
}
