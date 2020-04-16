using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Threading.Tasks;

namespace comicDownLoad
{
    class YYSLManHua:PublicThing
    {
        public override DownLoadComic GetDownImageList(string response)
        {
            int index = 0;
            string fileName = "";
            string srcUrl = "";
            string basicUrl = "";
            List<string> urlList;
            urlList = new List<string>();
            DownLoadComic comic = new DownLoadComic();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            HtmlNode showNode = doc.DocumentNode.SelectSingleNode("//img[@id='caonima']");
            srcUrl = showNode.Attributes["src"].Value;
            index = srcUrl.LastIndexOf("/");
            basicUrl = srcUrl.Substring(0, index);
            fileName = srcUrl.Substring(srcUrl.LastIndexOf("."));
            Regex regex = new Regex(@"共(?<data>\d+)頁");
            int count = Convert.ToInt32(regex.Match(response).Groups["data"].Value);
            
            for (int i = 1; i < count; i++)
            {               
               urlList.Add(basicUrl + "/" + i.ToString().PadLeft(3,'0') + fileName);
            }

            comic.Count = count;
            comic.ImageList = urlList;
            return comic;
        }

        public override Queue<BasicComicInfo> GetTopComic(string response)
        {
            HtmlNode nodeTemp;
            BasicComicInfo basicInfo;
            Queue<BasicComicInfo> queue;
            queue = new Queue<BasicComicInfo>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            HtmlNodeCollection collect = doc.DocumentNode.SelectNodes("//a[@class='clip-link']");

            foreach (HtmlNode node in collect)
            {
                basicInfo = new BasicComicInfo();
                basicInfo.ComicName = node.Attributes["title"].Value;
                basicInfo.ComicHref = node.Attributes["href"].Value;
                nodeTemp = node.SelectSingleNode("./span/img");
                basicInfo.ComicImgUrl = nodeTemp.Attributes["src"].Value;
                queue.Enqueue(basicInfo);
            }

            return queue;
        }

        public override ComicInfo GetComicInfo(string response)
        {
            Dictionary<string, string> comicDict;
            ComicInfo comicInfo = new ComicInfo();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);

            comicDict = new Dictionary<string, string>();
            HtmlNodeCollection nodeCollection = doc.DocumentNode.SelectNodes("//td[@width='20%']/a");
            comicInfo.CoverImgUrl = doc.DocumentNode.SelectSingleNode("//td[@align='center']/img").Attributes["src"].Value;
            comicInfo.Description = doc.DocumentNode.SelectSingleNode("//td[@colspan='3']").InnerText;
            comicInfo.Tag = doc.DocumentNode.SelectSingleNode("//div[@id='extras']/a").InnerText;

            foreach (HtmlNode tempNode in nodeCollection)
            {
                comicDict.Add(tempNode.InnerText,tempNode.Attributes["href"].Value);
            }

            comicInfo.URLDictionary = comicDict;
            return comicInfo;
        }

        public override CategoryInfo GiveCategoryInfo(string response)
        {
            CategoryInfo info; 
            Dictionary<string, string> dict;
            info = new CategoryInfo();
            dict = new Dictionary<string, string>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//ul[@class='menu']");
            HtmlNodeCollection collect = node.SelectNodes("./li/a");

            foreach (HtmlNode temp in collect)
            {
                if (temp.InnerText != "openlaod" && dict.ContainsKey(temp.InnerText) == false)
                    dict.Add(temp.InnerText, temp.Attributes["href"].Value);
            }

            info.ComicList = dict;
            return info;
        }

        public override CategoryCollect FindComicByCategory(string cateGoryStr)
        {
            HtmlDocument doc;
            HtmlNode node;
            HtmlNodeCollection nodeCollect;
            CategoryCollect collect;
            BasicComicInfo basicInfo = null;
            Queue<BasicComicInfo> queue;
            queue = new Queue<BasicComicInfo>();
            collect = new CategoryCollect();
            doc = new HtmlDocument();

            doc.LoadHtml(cateGoryStr);
            nodeCollect = doc.DocumentNode.SelectNodes("//div[@class='nag cf']/div/div/a");
            
            foreach(HtmlNode temp in nodeCollect)
            {
                node = temp.SelectSingleNode("./span/img");
                basicInfo = new BasicComicInfo();
                basicInfo.ComicHref = temp.Attributes["href"].Value;
                basicInfo.ComicName = temp.Attributes["title"].Value;
                basicInfo.ComicImgUrl = node.Attributes["src"].Value;
                queue.Enqueue(basicInfo);
            }

            collect.ComicQueue = queue;
            collect.Count = queue.Count;
            node = doc.DocumentNode.SelectSingleNode("//a[@class='nextpostslink']");
            if(node != null)
            collect.NextPageUrl = node.Attributes["href"].Value;
            
            return collect;
        }
      
    }
}
