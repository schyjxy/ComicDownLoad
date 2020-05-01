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
            HtmlNodeCollection nodes = mainNode.SelectNodes("//div[@class='index-manga-item']");

            if(nodes != null)
            {
                foreach(HtmlNode temp in nodes)
                {
                    basicComicInfo = new BasicComicInfo();
                    basicComicInfo.ComicHref = host + temp.SelectSingleNode("./a").Attributes["href"].Value;
                    basicComicInfo.ComicName = temp.SelectSingleNode("./p/a").InnerText;
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

            HtmlNode node = mainNode.SelectSingleNode("//p[@class='detail-info-tip']");
            comicInfo.Author = node.SelectSingleNode("./span/a").InnerText;
            comicInfo.HasFinished = node.SelectNodes("./span")[1].SelectSingleNode("./span").InnerText;
            comicInfo.Tag = node.SelectNodes("./span")[2].SelectSingleNode("./span").InnerText;

            node = mainNode.SelectSingleNode("//p[@class='detail-info-content']");
            if(node != null)
            {
                comicInfo.Description = node.InnerText;
            }
            else
            {
                comicInfo.Description = node.SelectNodes("./span")[2].SelectSingleNode("./span").InnerText;
            }
            
            comicInfo.URLDictionary = new Dictionary<string, string>();

            HtmlNodeCollection htmlNodes = mainNode.SelectNodes("//div[@class='detail-list-form-con']/a");

            if (htmlNodes != null)
            {
                foreach(HtmlNode temp in htmlNodes)
                {
                    string name = temp.InnerText.Replace(" ", "");

                    if (!comicInfo.URLDictionary.ContainsKey(name))
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
            HtmlNodeCollection nodes = mainNode.SelectNodes("//div[@class='mh-item']");

            if (nodes != null)
            {
                foreach (HtmlNode temp in nodes)
                {
                    basicComicInfo = new BasicComicInfo();
                    basicComicInfo.ComicHref = host + temp.SelectSingleNode("./a").Attributes["href"].Value;
                    basicComicInfo.ComicName = temp.SelectSingleNode("./div/h2/a").InnerText;
                    basicComicInfo.ComicImgUrl = temp.SelectSingleNode("./a/img").Attributes["src"].Value;
                    queue.Enqueue(basicComicInfo);
                }
            }

            int curIndex = 0;
            int count = 1;
            nodes = mainNode.SelectNodes("//div[@class='page-pagination']/ul/li");
            collect.PagesCollection = new Dictionary<string, string>();

            if(nodes != null)
            {
                foreach(HtmlNode temp in nodes)//这部分有问题，找不到page-pagination段
                {
                    if (!collect.PagesCollection.ContainsKey(temp.InnerText))
                    {
                        HtmlNode node = temp.SelectSingleNode("./a");

                        if (node.Attributes["class"] != null)
                        {
                            curIndex = count;
                        }

                        collect.PagesCollection.Add(node.InnerText, host + node.Attributes["href"].Value);
                        count++;
                    }
                }
            }

            collect.ComicTotalCount = queue.Count;
            collect.ComicQueue = queue;

            if (curIndex - 1 > 0 && collect.Count > 0)
            {
                collect.LastPageUrl = collect.PagesCollection[(curIndex - 1).ToString()];
            }
            else
            {
                collect.LastPageUrl = collect.PagesCollection[(curIndex).ToString()];
            }

            if (curIndex + 1 < collect.PagesCollection.Count && collect.Count > 0)
            {
                collect.NextPageUrl = collect.PagesCollection[(curIndex + 1).ToString()];
            }
            else
            {
                collect.NextPageUrl = collect.PagesCollection[(collect.PagesCollection.Count-1).ToString()];
            }

           
            return collect;
        }

    
        public override DownLoadComic GetDownImageList(string response)
        {
            DownLoadComic downLoad = new DownLoadComic();
            Regex urlRegex = new Regex(@"MANGABZ_CURL\s*=\s*""(?<data>[\w/]*)""");
            Regex midRegex = new Regex(@"MANGABZ_MID\s*=\s*(?<data>\d+)");
            Regex cidRegex = new Regex(@"MANGABZ_CID=(?<data>\d+)");
            Regex signRegex = new Regex(@"MANGABZ_VIEWSIGN=""(?<data>\w+)""");
            Regex dtRegex = new Regex(@"MANGABZ_VIEWSIGN_DT=""(?<data>[\w\s\-\:]*)""");
            Regex countRegex = new Regex(@"MANGABZ_IMAGE_COUNT\s*=\s*(?<data>\d+)");

            var url = "";
            var mid = "";
            var cid = "";
            var sign = "";
            var dt = "";
            var count = Convert.ToInt32(countRegex.Match(response).Groups["data"].Value);

            var requestUrl = "";
            string temp = "";
            List<string> urlList = new List<string>();
            urlList.Add(currentUrl);

            for(int i=1;i<count + 1;i++)
            {
                temp = currentUrl + "#ipg" + i;
                urlList.Add(temp);
            }

            int pos = 1;
            downLoad.ImageList = new List<string>();

            for (int i = 0;i<urlList.Count;i++)
            {
                response = AnalyseTool.HttpGet(urlList[i]);
                url = urlRegex.Match(response).Groups["data"].Value;
                mid = midRegex.Match(response).Groups["data"].Value;
                cid = cidRegex.Match(response).Groups["data"].Value;
                sign = signRegex.Match(response).Groups["data"].Value;

                requestUrl = host + string.Format("{0}chapterimage.ashx?cid={1}&page={2}&key=&" +
               "_cid={1}&_mid={3}&_dt={4}&sign={5}", url, cid, pos++, mid, System.Web.HttpUtility.UrlEncode(dt), sign);

                var msg = AnalyseTool.HttpGet(requestUrl, currentUrl);

                if(msg == "")
                {
                    msg = AnalyseTool.HttpGet(requestUrl, currentUrl);
                }

                url = urlRegex.Match(response).Groups["data"].Value;

                if (msg != null && msg.Length > 0)
                {
                    Microsoft.JScript.ArrayObject obj = AnalyseTool.EvalJScript(msg);
                    int len = Convert.ToInt32(obj.length);

                    for (int k = 0; k < len; k++)
                    {
                        string imgUrl = obj[k].ToString();

                        if (downLoad.ImageList.Exists(o=> o== imgUrl) == false)
                        {
                            downLoad.ImageList.Add(imgUrl);
                        }                      
                        
                    }
                }
            }

            downLoad.Count = downLoad.ImageList.Count;
            return downLoad;
        }

        public override CategoryInfo GiveCategoryInfo(string response)
        {
            string name = "";
            HtmlNode mainNode = GetMainNode(response);
            CategoryInfo info = new CategoryInfo();
            info.ComicList = new Dictionary<string, string>(0);
            HtmlNodeCollection htmlNodes = mainNode.SelectNodes("//div[@class='class-line']");

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
