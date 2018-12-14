using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace comicDownLoad
{
    class ManHuaRen:VeryDm
    {
        string hostAttach = "http://www.1kkk.com";

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
            comicInfo.Tag = nodeTemp.SelectNodes("./span")[1].InnerText.Replace("题材：", "").Replace(" ","");
            comicInfo.Description = node.SelectSingleNode("./p[@class='content']").InnerText;

            node = doc.DocumentNode.SelectSingleNode("//div[@id='chapterlistload']/ul");
           
            foreach (HtmlNode tempNode in node.SelectNodes("./li/a"))
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
            var count = 0;
            var index = 0;

            foreach(HtmlNode temp in collect)
            {
                count++;
                tempStr = temp.Attributes["data-index"].Value;
                
                if(dict.ContainsKey(tempStr) == false)
                {
                    dict.Add(tempStr, hostAttach + temp.Attributes["href"].Value);
                }

                if (temp.Attributes["class"] != null && temp.Attributes["class"].Value == "active")
                {
                    index = count;
                }

    
            }

            collectInfo.NextPageUrl = dict[(index + 1).ToString()];
            collectInfo.ComicQueue = queue;
            collectInfo.Count = queue.Count;
            collectInfo.PagesCollection = dict;
            return collectInfo;
        }


        private List<string> ExecuteImageList(string data)
        {
            List<string> list = new List<string>();
            return list;
        }

        private string GenerateAjax(string response)
        {
            var DM5_CID = "";
            var DM5_MID = "";
            var DM5_PAGE = "";
            var DM5_ViewSign = "";
            var DM5_ViewSignDt = "";
            var DM5_CURL = "";
            var ImageCount = "";
            var retStr = "";

            DM5_CID = Regex.Match(response, @"DM5_CID\s*=\s*(?<data>\d+)").Groups["data"].Value;
            DM5_MID = Regex.Match(response, @"DM5_MID\s*=\s*(?<data>\d+)").Groups["data"].Value;
            DM5_PAGE = Regex.Match(response, @"DM5_PAGEINDEX\s*=\s*(?<data>\d+)").Groups["data"].Value;
            DM5_ViewSign = Regex.Match(response, @"DM5_VIEWSIGN\s*=\s*""(?<data>\w+)""").Groups["data"].Value;
            DM5_ViewSignDt = Regex.Match(response, @"DM5_VIEWSIGN_DT\s*=\s*""(?<data>[\w\-\s\:\+\%]*)""").Groups["data"].Value;
            DM5_CURL = Regex.Match(response, @"DM5_CURL\s*=\s*""(?<data>[\w-/]*)""").Groups["data"].Value;
            ImageCount = Regex.Match(response, @"DM5_IMAGE_COUNT\s*=\s*(?<data>\d+)").Groups["data"].Value;

            retStr = hostAttach + DM5_CURL + "chapterfun.ashx?cid=" + DM5_CID + "&page=" + DM5_PAGE + "&key=" + ""
                     + "&language=1&gtk=6&_cid=" + DM5_CID + "&_mid=" + DM5_MID + "&_dt=" + DM5_ViewSignDt + "&_sign=" + DM5_ViewSign;

            retStr = AnalyseTool.HttpGet(retStr, hostAttach + DM5_CURL);
            DecodeOne(retStr);
            return retStr;
        }

        private void DecodeOne(string data)
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
            ret = DecodeEncryUrl(p, a, c, dicArry);
            DecodeTwo(ret);
        }

        private void DecodeTwo(string data)
        {
            var cid = "";
            var key = "";

        }

        public override DownLoadComic GetDownImageList(string response)
        {
            string ajaxStr = "";
            string temp = "";
            HtmlDocument doc;
            doc = new HtmlDocument();
            doc.LoadHtml(response);
            DownLoadComic down = new DownLoadComic();           
            ajaxStr = GenerateAjax(response);

            return down;
        }


    }
}
