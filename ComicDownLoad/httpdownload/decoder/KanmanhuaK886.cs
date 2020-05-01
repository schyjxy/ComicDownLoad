using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using comicDownLoad;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace comicDownLoad
{
    class KanmanhuaK886:PublicThing
    {
        //获取热门漫画
        string hostName = "https://www.k886.net/";

        public override Queue<BasicComicInfo> GetTopComic(string response)
        {
            HtmlDocument doc;
            HtmlNodeCollection tempCollect;
            BasicComicInfo basic = null;
            Queue<BasicComicInfo> queue = null;
            Dictionary<string,string> dict;
            doc = new HtmlDocument();
            dict = new Dictionary<string,string>();
            queue = new Queue<BasicComicInfo>();
            doc.LoadHtml(response);
            HtmlNodeCollection collect = doc.DocumentNode.SelectNodes("//ul[@class='liemh indliemh']");
            
            foreach (HtmlNode node in collect)
            {
                tempCollect = node.SelectNodes("./li/a");
                
                foreach (HtmlNode node1 in tempCollect)
                {
                    if (dict.ContainsKey(node1.Attributes["title"].Value) == false)
                    {
                        basic = new BasicComicInfo();
                        basic.ComicHref = node1.Attributes["href"].Value;
                        basic.ComicName = node1.Attributes["title"].Value;
                        basic.ComicImgUrl = node1.SelectSingleNode("./img").Attributes["src"].Value;
                        queue.Enqueue(basic);
                    }
                }
            }
            return queue;
        }

        public override ComicInfo GetComicInfo(string response)
        {
            HtmlDocument doc;
            HtmlNodeCollection tempCollect;
            ComicInfo info = new ComicInfo();
            Dictionary<string, string> dict;

            dict = new Dictionary<string, string>();
            doc = new HtmlDocument();
            doc.LoadHtml(response);

            tempCollect = doc.DocumentNode.SelectNodes("//dl[@class='mh-detail']/dd/p");
            info.Author = tempCollect[0].SelectSingleNode("./a").InnerText;
            info.HasFinished = tempCollect[1].SelectSingleNode("./a").InnerText;

            tempCollect = doc.DocumentNode.SelectNodes("//div[@class='mh-introduce']/p");
            info.Description = AnalyseTool.ReplacePunctuation(tempCollect[1].InnerText);

            tempCollect = doc.DocumentNode.SelectNodes("//ul[@class='b1']");
            
            foreach (HtmlNode temp in tempCollect)
            {
                HtmlNodeCollection collect = temp.SelectNodes("./li/a");

                foreach (HtmlNode node in collect)
                {
                    if (dict.ContainsKey(node.Attributes["title"].Value) == false)
                    {
                        dict.Add(node.Attributes["title"].Value, node.Attributes["href"].Value);
                    }
                }
            }

            info.URLDictionary = dict;
            return info;
        }
        //需要修改路径和标签
        public override CategoryInfo GiveCategoryInfo(string response)
        {
            string href = "";
            CategoryInfo cateInfo = new CategoryInfo();
            HtmlDocument doc = new HtmlDocument();
            Dictionary<string, string> cateDict;
            doc.LoadHtml(response);
            cateDict = new Dictionary<string, string>();

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@class='panels shadow-gray']");
            doc.LoadHtml(node.OuterHtml);
            HtmlNodeCollection collect = node.SelectNodes("//a");

            foreach (HtmlNode temp in collect)
            {
                href = temp.Attributes["href"].Value;

                if (cateDict.ContainsKey(temp.InnerText) == false && href != "#")
                    cateDict.Add(temp.InnerText, href);
            }

            cateInfo.ComicList = cateDict;
            return cateInfo;
        }

        //需要修改路径和标签
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

            collect = doc.DocumentNode.SelectNodes("//ul[@class='liemh htmls indliemh']/li");

            foreach (HtmlNode temp in collect)
            {
                basicInfo = new BasicComicInfo();
                node = temp.SelectSingleNode("./a");
                basicInfo.ComicName = node.Attributes["title"].Value;
                basicInfo.ComicHref = node.Attributes["href"].Value;
                basicInfo.ComicImgUrl = temp.SelectSingleNode("./a/img").Attributes["src"].Value;
                comicQueue.Enqueue(basicInfo);
            }

            node = doc.DocumentNode.SelectSingleNode("//div[@class='pagination-wrapper']");
            cateCollect.NextPageUrl = hostName + node.SelectNodes("./a[@class='next']")[0].Attributes["href"].Value;
            cateCollect.Count = comicQueue.Count;
            cateCollect.ComicQueue = comicQueue;
            return cateCollect;
        }

        public override DownLoadComic GetDownImageList(string response)
        {
            string nextHtml = "";
            List<string> imageList;
            HtmlDocument doc;
            HtmlNode node;
            HtmlNodeCollection nodeCollect;
            DownLoadComic comic;

            doc = new HtmlDocument();
            doc.LoadHtml(response);
            comic = new DownLoadComic();
            imageList = new List<string>();

            try
            {
                nodeCollect = doc.DocumentNode.SelectNodes("//img");
                node = doc.DocumentNode.SelectSingleNode("//input[@id='total']");
                var total = Convert.ToInt32(node.Attributes["value"].Value);

                node = doc.DocumentNode.SelectSingleNode("//input[@id='pos']");
                var pos = Convert.ToInt32(node.Attributes["value"].Value) + 1;
                var cid = Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//input[@id='cid']").Attributes["value"].Value);
                var id = Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//input[@id='id']").Attributes["value"].Value);

                nextHtml = hostName + "/index-look-cid-" + cid.ToString() + "-id-" + id.ToString() + "-p-" + pos.ToString();

                if (nodeCollect != null && nodeCollect.Count >= 2)
                {
                    imageList = FillImageList(nodeCollect[0].Attributes["src"].Value, nodeCollect[1].Attributes["src"].Value, total);

                    if (imageList.Count == total)
                    {
                        comic.Count = imageList.Count;
                        comic.ImageList = imageList;
                        return comic;
                    }
                }

                foreach (HtmlNode temp in nodeCollect)
                {
                    imageList.Add(temp.Attributes["src"].Value);
                }

                for (int i = 0; i < total; i++)
                {
                    response = AnalyseTool.HttpGet(nextHtml,Encoding.UTF8);
                    doc.LoadHtml(response);
                    node = doc.DocumentNode.SelectSingleNode("//input[@id='pos']");
                    pos = Convert.ToInt32(node.Attributes["value"].Value) + 1;
                    cid = Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//input[@id='cid']").Attributes["value"].Value);
                    id = Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//input[@id='id']").Attributes["value"].Value);
                    nextHtml = hostName + "/index-look-cid-" + cid.ToString() + "-id-" + id.ToString() + "-p-" + pos.ToString();                    
                    nodeCollect = doc.DocumentNode.SelectNodes("//img");

                    foreach (HtmlNode temp in nodeCollect)
                    {
                        if (imageList.Contains(temp.Attributes["src"].Value) == false)
                            imageList.Add(temp.Attributes["src"].Value);
                    }
                   
                    if (imageList.Count >= total || pos == total)
                        break;
                }

                comic.Count = imageList.Count;
                comic.ImageList = imageList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("KanManHua{0}.GetDownImageList,原因:{1}", ex.Message);
            }
            return comic;
        }

        private List<string> FillImageList(string url1, string url2,int count)
        {
            var startIndex = 0;
            var index = 0;
            var url = "";
            var temp = "";
            var attachName = "";
            var basicUrl = "";
            List<string> imageList = new List<string>();
            Regex regex = new Regex(@"\D");
                 
            index = url1.LastIndexOf("/")+1;
            basicUrl = url1.Substring(0, index);
            temp = url1.Substring(index, url1.LastIndexOf(".") - index);

            if (regex.IsMatch(temp))
            {
                return imageList;
            }

            startIndex = Convert.ToInt32(temp);
            attachName = url1.Substring(url1.LastIndexOf("."));

            for (int i = startIndex; i < startIndex + count; i++)
            {
                url = basicUrl + i.ToString() + attachName;
                imageList.Add(url);
            }

            return imageList;
        }

        public override SearchResult GetSearchComic(string response)
        {
            SearchResult result = new SearchResult();
            HtmlDocument doc = new HtmlDocument();
            Queue<BasicComicInfo> queue;
            queue = new Queue<BasicComicInfo>();
            doc.LoadHtml(response);
            BasicComicInfo info;
            HtmlNodeCollection collect = doc.DocumentNode.SelectNodes("//ul[@class='liemh htmls indliemh']/li");
            if (collect == null)
            {
                return result;
            }
            
            foreach (HtmlNode node in collect)
            {
                info = new BasicComicInfo();
                info.ComicHref = node.SelectSingleNode("./a").Attributes["href"].Value;
                info.ComicName = node.SelectSingleNode("./a").Attributes["title"].Value;
                info.ComicImgUrl = node.SelectSingleNode("./a/img").Attributes["src"].Value;
                queue.Enqueue(info);
            }

            HtmlNode nodeTemp = doc.DocumentNode.SelectSingleNode("//div[@class='position']");
            Regex regex = new Regex(@"(?<data>\d+)");
            result.Count = Convert.ToInt32(regex.Match(nodeTemp.InnerText).Groups["data"].Value);

            collect = doc.DocumentNode.SelectNodes("//a[@class='next']");

            if (collect != null)
                result.NextPageUrl = hostName + collect[0].Attributes["href"].Value;
            else
                result.NextPageUrl = "";

            result.SearchQueue = queue;
            return result;
        }

    }
}
