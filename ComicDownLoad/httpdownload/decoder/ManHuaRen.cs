using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace comicDownLoad
{
    class ManHuaRen:VeryDm
    {
        string hostAttach = "http://www.1kkk.com";
        Dictionary<string, string> dict;

        public override Queue<BasicComicInfo> GetTopComic(string response)
        {
            string temp;
            HtmlNode node;
            HtmlDocument doc;
            doc = new HtmlDocument();
            doc.LoadHtml(response);
            Queue<BasicComicInfo> queue;
            BasicComicInfo basicInfo;
            queue = new Queue<BasicComicInfo>();

            HtmlNodeCollection nodeCollect = doc.DocumentNode.SelectNodes("//div[@class='mh-item']");
            
            foreach (HtmlNode n in nodeCollect)
            {
                node = n.SelectSingleNode("./p[@class='mh-cover ']");
                if (node == null)
                    continue;
                temp = node.Attributes["style"].Value;
                basicInfo = new BasicComicInfo();
                basicInfo.ComicImgUrl = temp.Substring(temp.IndexOf("(")+1, temp.Length - temp.IndexOf("(") - 2);
                node = n.SelectSingleNode("./div[@class='mh-item-detali']");
                basicInfo.ComicName = node.SelectSingleNode("./h2/a").Attributes["title"].Value;
                basicInfo.ComicHref = hostAttach + node.SelectSingleNode("./h2/a").Attributes["href"].Value;
                queue.Enqueue(basicInfo);
            }
            return queue;
        }

        public override ComicInfo GetComicInfo(string response)
        {
            string temp;
            HtmlDocument doc;
            doc = new HtmlDocument();
            doc.LoadHtml(response);
            ComicInfo comicInfo = new ComicInfo();
            Dictionary<string, string> dict;
            dict = new Dictionary<string, string>();
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@class='banner_detail_form']/div[@class='info']");
            comicInfo.Author = node.SelectSingleNode("./p[@class='subtitle']").InnerText.Replace("作者：", "");
            comicInfo.otherWork = hostAttach + node.SelectSingleNode("./p[@class='subtitle']/a").Attributes["href"].Value;

            HtmlNode nodeTemp = node.SelectSingleNode("./p[@class='tip']");
            comicInfo.HasFinished = nodeTemp.SelectNodes("./span")[0].InnerText.Replace("状态：", "");
            
            if(nodeTemp.SelectNodes("./span").Count == 2)
            {
                comicInfo.Tag = nodeTemp.SelectNodes("./span")[1].InnerText.Replace("题材：", "").Replace(" ", "");
            }
            
            comicInfo.Description = node.SelectSingleNode("./p[@class='content']").InnerText;
            node = doc.DocumentNode.SelectSingleNode("//div[@id='chapterlistload']/ul");

            foreach (HtmlNode tempNode in node.SelectNodes("./li/a|./ul/li/a"))
            {
                temp = tempNode.InnerText.Replace(" ", "");

                if (dict.ContainsKey(temp) == false)
                {
                    dict.Add(temp, hostAttach + tempNode.Attributes["href"].Value);
                }
            }
            comicInfo.URLDictionary = dict;
            return comicInfo;
        }

        public override CategoryInfo GiveCategoryInfo(string response)
        {
            HtmlNodeCollection nodeCollect;
            CategoryInfo info;
            HtmlDocument doc;
            doc = new HtmlDocument();
            doc.LoadHtml(response);
            info = new CategoryInfo();
            Dictionary<string, string> dict;
            dict = new Dictionary<string, string>();
            nodeCollect = doc.DocumentNode.SelectNodes("//dl[@class='cat-list']/dd/a");
            
            foreach (HtmlNode node in nodeCollect)
            {
                if (dict.ContainsKey(node.InnerText) == false)
                {
                    dict.Add(node.InnerText, hostAttach + node.Attributes["href"].Value);
                }
            }

            info.ComicList = dict;
            return info;
        }

        public override SearchResult GetSearchComic(string response)
        {
            BasicComicInfo basic;
            SearchResult result = new SearchResult();
            HtmlNodeCollection nodeCollect;
            HtmlDocument doc;
            doc = new HtmlDocument();
            doc.LoadHtml(response);
            nodeCollect = doc.DocumentNode.SelectNodes("//div[@class='mh-item']");
            result.Count = 0;    

            string strTemp;
            HtmlNode subNode;
            Queue<BasicComicInfo> queue;
            queue = new Queue<BasicComicInfo>();

            if (nodeCollect == null)
            {
                return result;
            }

            subNode = doc.DocumentNode.SelectSingleNode("//header[@class='box-header']");
            strTemp = subNode.SelectSingleNode("./h1/span").InnerText;
            strTemp = Regex.Match(strTemp, @"\d+").Value;
            result.Count = Convert.ToInt32(strTemp);

            foreach (HtmlNode temp in nodeCollect)
            {
                basic = new BasicComicInfo();
                strTemp = temp.SelectSingleNode("./p").Attributes["style"].Value;
                basic.ComicImgUrl = strTemp.Substring(strTemp.IndexOf("(") + 1, strTemp.Length - strTemp.IndexOf("(") - 2 );
                subNode = temp.SelectSingleNode("./div/h2/a");
                basic.ComicName = subNode.Attributes["title"].Value;
                basic.ComicHref = hostAttach + subNode.Attributes["href"].Value;
                queue.Enqueue(basic);
            }

            nodeCollect = doc.DocumentNode.SelectNodes("//div[@class='page-pagination pull-right mt20']/ul/li/a");
            int index = 0;

            foreach (HtmlNode temp in nodeCollect)
            {
                if(temp.Attributes["class"] != null && temp.Attributes["class"].Value == "active")
                {
                    result.NextPageUrl = hostAttach + nodeCollect[++index].Attributes["href"].Value;
                    break;
                }

                index++;
            }

            result.SearchQueue = queue;
            return result;
        }

        public override CategoryCollect FindComicByCategory(string cateGoryStr)
        {
            string tempStr = "";
            HtmlDocument doc;
            HtmlNode node;
            BasicComicInfo basic;
            Dictionary<string,string> dict;
            Queue<BasicComicInfo> queue;
            CategoryCollect collectInfo;
            queue = new Queue<BasicComicInfo>();
            collectInfo = new CategoryCollect();
            doc = new HtmlDocument();
            doc.LoadHtml(cateGoryStr);
            HtmlNodeCollection collect = doc.DocumentNode.SelectNodes("//div[@class='box-body']/ul[@class='mh-list col7']/li/div[@class='mh-item']");
            
            foreach (HtmlNode temp in collect)
            {
                basic = new BasicComicInfo();
                node = temp.SelectSingleNode("./div[@class='mh-item-detali']/h2[@class='title']/a");
                basic.ComicName = node.Attributes["title"].Value;
                basic.ComicHref = hostAttach + node.Attributes["href"].Value;
                node = temp.SelectSingleNode("./p[@class='mh-cover']");
                tempStr = node.Attributes["style"].Value;
                basic.ComicImgUrl = tempStr.Substring(tempStr.IndexOf("(") + 1, tempStr.IndexOf(")") - tempStr.IndexOf("(") -1 );
                queue.Enqueue(basic);
            }

            node = doc.DocumentNode.SelectSingleNode("//div[@class='page-pagination pull-right mt20']");
            collect = node.SelectNodes("./ul/li/a");
            dict = new Dictionary<string,string>();
            var page = 1;
            var index = 1;

            foreach(HtmlNode temp in collect)
            {               
                tempStr = temp.Attributes["data-index"].Value;
                
                if(dict.ContainsKey(tempStr) == false)
                {
                    dict.Add(tempStr, hostAttach + temp.Attributes["href"].Value);
                }

                if (temp.Attributes["class"] != null && temp.Attributes["class"].Value == "active")
                {
                    page = index;
                }

                index++;
            }

            if (dict.ContainsKey((page + 1).ToString()))
                collectInfo.NextPageUrl = dict[(page + 1).ToString()];

            collectInfo.ComicQueue = queue;
            collectInfo.Count = queue.Count;
            collectInfo.PagesCollection = dict;
            return collectInfo;
        }

        private List<string> GenerateImage(string response)
        {
            var DM5_CID = "";
            var DM5_MID = "";
            var DM5_PAGE = "";
            var DM5_ViewSign = "";
            var DM5_ViewSignDt = "";
            var DM5_CURL = "";
            var ImageCount = "";
            var retStr = "";

            List<string> dataList;
            dataList = new List<string>();

            DM5_CURL = Regex.Match(response, @"DM5_CURL\s*=\s*""(?<data>[\w-/]*)""").Groups["data"].Value;
            var nextPageUrl = hostAttach + DM5_CURL.Substring(0, DM5_CURL.LastIndexOf("/")) + "-p2";
          
            DM5_CURL = Regex.Match(response, @"DM5_CURL\s*=\s*""(?<data>[\w-/]*)""").Groups["data"].Value;
            DM5_CID = Regex.Match(response, @"DM5_CID\s*=\s*(?<data>\d+)").Groups["data"].Value;
            DM5_MID = Regex.Match(response, @"DM5_MID\s*=\s*(?<data>\d+)").Groups["data"].Value;
            DM5_PAGE = Regex.Match(response, @"DM5_PAGEINDEX\s*=\s*(?<data>\d+)").Groups["data"].Value;
            DM5_ViewSign = Regex.Match(response, @"DM5_VIEWSIGN\s*=\s*""(?<data>\w+)""").Groups["data"].Value;
            DM5_ViewSignDt = Regex.Match(response, @"DM5_VIEWSIGN_DT\s*=\s*""(?<data>[\w\-\s\:\+\%]*)""").Groups["data"].Value;
            DM5_ViewSignDt = System.Web.HttpUtility.UrlEncode(DM5_ViewSignDt, System.Text.Encoding.UTF8);
            ImageCount = Regex.Match(response, @"DM5_IMAGE_COUNT\s*=\s*(?<data>\d+)").Groups["data"].Value;

            var nextDir = DM5_CURL;
            var jsUrl = hostAttach + nextDir + "chapterfun.ashx?cid=" + DM5_CID + "&page=" + DM5_PAGE + "&key=" + ""
                     + "&language=1&gtk=6&_cid=" + DM5_CID + "&_mid=" + DM5_MID + "&_dt=" + DM5_ViewSignDt + "&_sign=" + DM5_ViewSign;

            retStr = AnalyseTool.HttpGet(jsUrl);
            
            if (retStr == "")
            {
                return null;
            }

            int total = Convert.ToInt32(ImageCount);
            dataList = DecodeOne(retStr);

            if (dataList.Count < Convert.ToInt32(ImageCount))
            {
                response = AnalyseTool.HttpGet(nextPageUrl);
                dataList.AddRange(GenerateImage(response));
            }
            return dataList;
        }

        private List<string> DecodeOne(string data)
        {
            var a = 0;
            var c = 0;
            var p = "";
            var ret = "";
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
            var dic = temp.Substring(temp.IndexOf(c.ToString() + ",'") + 4);
            dic = dic.Substring(0, dic.LastIndexOf("'"));
            dicArry = dic.Split('|');
            p = temp.Substring(1,temp.IndexOf(a.ToString()) -4);
            p = p.Replace("\\'", "'");
            ret = DecodeEncryUrl(p, a, c, dicArry);
            return GetImageUrl(ret);
        }

        private string XorStr(string dataA, string dataB)
        {
            if (dataA != "")
                return dataA;
            else if (dataB != "")
                return dataB;
            else
                return "";
        }


        private string DecodeEncryUrl(string p, int a, int c, string[] k)
        {
            Regex regex;
            string replaceText = "";
            dict = new Dictionary<string,string>();

            while (c-- > 0)
            {
                dict.Add(eFun(c,a), XorStr(k[c], eFun(c,a)));
            }

            c = 1;
            replaceText = "(?<data>" +  "\\b" + "\\w+" + "\\b" + ")";
            regex = new Regex(replaceText);
            List<string> dataList = new List<string>();

            while (c-- > 0)
            {
                if (dict.ContainsKey(c.ToString()))
                {
                    p = Regex.Replace(p, replaceText, GetWord);
                }
            }

            dict.Clear();
            return p;
        }

        private string GetWord(Match m)
        {
            string data = "";

            if (dict.ContainsKey(m.Groups["data"].Value))
            {
                data = dict[m.Groups["data"].Value];
            }
            return data;
        }

        private List<string> GetImageUrl(string data)
        {
            var cid = "";
            var key = "";
            var pix = "";
            var pvalue = "";
            var fullLink = "";
            var temp = "";
            List<string> dataList = new List<string>();
            cid = Regex.Match(data, @"cid=(?<data>\d+)").Groups["data"].Value;
            key = Regex.Match(data, @"key=\'(?<data>\w+)\'").Groups["data"].Value;
            pix = Regex.Match(data, @"pix=\""(?<data>[\w\:\.\-/]*)\""").Groups["data"].Value;
            pvalue = Regex.Match(data, @"pvalue=\[(?<data>[""/\w\.\,]*)\]").Groups["data"].Value;
            string[] url = pvalue.Split(',');

            foreach (var i in url)
            {
                temp = i.Replace("\"", "");
                fullLink = pix + temp + "?cid=" + cid + "&key=" + key;
                dataList.Add(fullLink);
            }
            return dataList;

        }

        public override DownLoadComic GetDownImageList(string response)
        {         
            HtmlDocument doc;
            doc = new HtmlDocument();
            doc.LoadHtml(response);
            DownLoadComic down = new DownLoadComic();           
            List<string> dataList = GenerateImage(response);//生成图片
            down.Count = dataList.Count;
            down.ImageList = dataList;
            return down;
        }
    }
}
