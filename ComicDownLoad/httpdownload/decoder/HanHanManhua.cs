using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace comicDownLoad
{
    class HanHanManhua:PublicThing
    {
        private string locationHref;
        public string hostResponse;
        private string hostAttach = "http://www.hhimm.com";

        public override Queue<BasicComicInfo> GetTopComic(string response)//实现热门
        {
            BasicComicInfo basic = null;
            Queue<BasicComicInfo> queue = new Queue<BasicComicInfo>();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(response);
            HtmlNode node = document.DocumentNode.SelectSingleNode("//div[@class='cComicHot']");
            HtmlNodeCollection collect = node.SelectNodes("./div/li/a");

            foreach (var i in collect)
            {              
                basic = new BasicComicInfo();
                basic.ComicHref = hostAttach + i.Attributes["href"].Value;
                basic.ComicName = i.Attributes["title"].Value;
                basic.ComicImgUrl = i.SelectSingleNode("./img").Attributes["src"].Value;
                queue.Enqueue(basic);
            }

            return queue;
        }

        public override CategoryInfo GiveCategoryInfo(string response)//给出目录连接
        {
            CategoryInfo category = new CategoryInfo();
            var comicDict = new Dictionary<string, string>();
            Regex regex = new Regex(@"href='(?<href>[/\w_.]*)'>(?<title>\w+)</a></span>");
            MatchCollection collect = regex.Matches(response);

            foreach (Match m in collect)
            {
                if (comicDict.ContainsKey(m.Groups["title"].Value) == false)
                    comicDict.Add(m.Groups["title"].Value, hostAttach + m.Groups["href"].Value);
            }

            category.ComicList = comicDict;
            return category;
        }

        public override CategoryCollect FindComicByCategory(string cateGoryStr)//实现漫画目录图片枚举
        {
            Regex regex;
            BasicComicInfo basic;
            CategoryCollect category = new CategoryCollect();
            var comicQueue = new Queue<BasicComicInfo>();          
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(cateGoryStr);
            HtmlNode node = document.DocumentNode.SelectSingleNode("//div[@class='cComicList']");
            HtmlNodeCollection collect = node.SelectNodes("./li/a");

            foreach (var i in collect)
            {
                basic = new BasicComicInfo();
                basic.ComicHref = hostAttach + i.Attributes["href"].Value;
                basic.ComicName = i.Attributes["title"].Value;
                basic.ComicImgUrl = i.SelectSingleNode("./img").Attributes["src"].Value;
                comicQueue.Enqueue(basic);
            }

            regex = new Regex(@"href='(?<href>[\w:/.]*)'>\s*下一页");//还可以修改，暂时不动
            category.NextPageUrl = hostAttach + regex.Match(cateGoryStr).Groups["href"].Value;
            category.ComicQueue = comicQueue;
            category.PagesCollection = new Dictionary<string, string>();
            return category;
        }

        public override DownLoadComic GetDownImageList(string response)
        {
            var retMsg = "";
            DownLoadComic comic = new DownLoadComic();
            Regex []regex = new Regex[4];
            regex[0] = new Regex(@"id=""hdPageCount""\svalue=""(?<count>\d+)""");
            regex[1] = new Regex(@"id=""hdVolID""\svalue=""(?<sid>\d+)""");
            regex[2] = new Regex(@"id=""hdS""\svalue=""(?<hds>\d+)""");
            regex[3] = new Regex(@"id=""hdPageIndex""\svalue=""(?<index>\d+)""");
            var comicCout = Convert.ToInt32(regex[0].Match(response).Groups["count"].Value);
            var sid = Convert.ToInt32(regex[1].Match(response).Groups["sid"].Value);
            var s = Convert.ToInt32(regex[2].Match(response).Groups["hds"].Value);
            var currtntIndex = Convert.ToInt32(regex[3].Match(response).Groups["index"].Value);

            List<string> urlList = new List<string>();
            List<string> htmlList = new List<string>();
            var html = "";

            for (int i = currtntIndex; i < comicCout; i++)
            {
                html = hostAttach + "/" + "cool" + sid + "/" + i.ToString()+ ".html?s=" + s;
                htmlList.Add(html);
            }

            string[] responseArry = AnalyseTool.HttpGet(htmlList.ToArray());
            foreach(var i in responseArry)
            {
                urlList.Add(GetImageUrl(i));
            }

            comic.ImageList = urlList;
            comic.Count = urlList.Count;
            return comic;           
        }

        public override SearchResult GetSearchComic(string response)
        {
            BasicComicInfo comicInfo;
            SearchResult result = new SearchResult();
            Queue<BasicComicInfo> queue = new Queue<BasicComicInfo>();
                      
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(response);

            if (response == "")
            {
                return result;
            }
            HtmlNode node = document.DocumentNode.SelectSingleNode("//div[@class='cComicList']");
            HtmlNodeCollection collect = node.SelectNodes("./li/a");
            
            if (collect == null)
            {
                result.Count = queue.Count;
                result.SearchQueue = queue;
                return result;
            }

            foreach (var i in collect)
            {
                comicInfo = new BasicComicInfo();
                comicInfo.ComicHref = hostAttach + i.Attributes["href"].Value;
                comicInfo.ComicName = i.Attributes["title"].Value;
                comicInfo.ComicImgUrl = i.SelectSingleNode("./img").Attributes["src"].Value;
                queue.Enqueue(comicInfo);
            }

            result.Count = queue.Count;
            result.SearchQueue = queue;
            result.NextPageUrl = "";
            return result;
        }

        public override ComicInfo GetComicInfo(string response)
        {
            string subMsg = "";
            ComicInfo comicInfo = new ComicInfo();
            Dictionary<string,string> comicDic;
            Regex regex;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);

            comicDic = new Dictionary<string,string>();
            regex = new Regex(@"src='(?<imgurl>[\w:/.]*)'");

            HtmlNodeCollection nodeCollection = doc.DocumentNode.SelectNodes("//a[@class='l_s']");
            
            foreach (HtmlNode node in nodeCollection)
            {
                if (comicDic.ContainsKey(node.Attributes["title"].Value) == false)
                {
                    comicDic.Add(node.Attributes["title"].Value, hostAttach + node.Attributes["href"].Value);
                }
            }

            HtmlNode currentNode = doc.DocumentNode.SelectSingleNode("//div[@id='about_kit']");
            nodeCollection = currentNode.SelectNodes("./ul/li");
            
            subMsg = AnalyseTool.GetTag(response, @"<div id=""about_style"">","</div>");
            comicInfo.CoverImgUrl = regex.Match(subMsg).Groups["imgurl"].Value;
            comicInfo.otherWork = "";
            comicInfo.Author = nodeCollection[1].InnerText.Trim().Replace("作者:","");
            comicInfo.HasFinished = nodeCollection[2].InnerText.Trim().Replace("状态:","");
            comicInfo.Description = nodeCollection[nodeCollection.Count - 1].InnerText.Trim().Replace("简介:","");
            comicInfo.URLDictionary = comicDic;
            return comicInfo;
        }

        private string GetUrlPar(string name)
        { 
            var reg = new Regex(@"(^|\\?|&)"+ name +"=([^&]*)(\\s|&|$)"); 
	        var href = "";//页面URl

            if (reg.IsMatch(href))
            {
                Match match = reg.Match(locationHref);
                return match.Groups[1].Value;
            }
            return "";
        }

        private string GetDomain(int s, string hostMsg)//获取漫画图片地址前面
        {
            string[] arrDs = GetArrDs(hostMsg);
            if(arrDs.Length == 1) 
                return arrDs[0];
            return arrDs[s];
        }

        private string GetKey(string webMsg)
        {
            string retKey = "";
            Regex[] regex = new Regex[4];
            regex[0] = new Regex(@"id=""img1021""\s*name=""(?<name>[\w/]*)""");
            regex[1] = new Regex(@"id=""img2391""\s*name=""(?<name>[\w/]*)""");
            regex[2] = new Regex(@"id=""img7652""\s*name=""(?<name>[\w/]*)""");
            regex[3] = new Regex(@"id=""imgCurr""\s*name=""(?<name>[\w/]*)""");

            for (var i = 0; i < regex.Length; i++)
            {
                if (regex[i].IsMatch(webMsg))
                {
                    retKey = regex[i].Match(webMsg).Groups["name"].Value;
                }
            }
              return retKey;
        }

        public string[] GetArrDs(string hostMsg)
        {
            string[] hostArry;
            Regex regex = new Regex(@"id=""hdDomain""\s*value=""(?<val>[:\w/.|]*)");
            string analyseInfo = regex.Match(hostMsg).Groups["val"].Value;
            hostArry = analyseInfo.Split('|');
            return hostArry;
        }

        public string GetImageUrl(string urlResponse)
        {
            string url = "";
            string key = GetKey(urlResponse);
            string cuDomainNo = GetUrlPar("d");

            if (cuDomainNo == "")
                cuDomainNo = "0";

            var sCuDomain = GetDomain(Convert.ToInt16(cuDomainNo), urlResponse);
            url = sCuDomain.Substring(0, sCuDomain.Length-1) + unsuan(key);//得到最终地址
            return url;
        }

        public string unsuan(string input)//解析出路径
        {
            var sw = "hhmmoo.com|hhssee.com";
            var su = "www.hhmmoo.com"; 
            bool b =  false;
            
            string[] dataArry = sw.Split('|');

            for (int i = 0; i < dataArry.Length; i++)
            {
                if (su.IndexOf(dataArry[i]) > -1)
                {
                    b = true;
                    break;
                }
            }

            if (!b) return "";

            var x = input.Substring(input.Length - 1);
            var w = "abcdefghijklmnopqrstuvwxyz";
            var xi = w.IndexOf(x) + 1;
            var sk = input.Substring(input.Length - xi - 12, 11);
            input = input.Substring(0, input.Length - xi - 12);
            var k = sk.Substring(0, sk.Length - 1);
            var f = sk.Substring(sk.Length-1);

            for (int i = 0; i < k.Length; i++)
            {
                input = input.Replace(k.Substring(i,1), i.ToString());
            }

            byte temp = 0;
            Encoding unicodeEncode = Encoding.ASCII;
            string[] ss = input.Split('x');
            string ret = "";

            for (int i = 0; i < ss.Length; i++)
            {
                temp = Convert.ToByte(ss[i]);
                ret = ret + unicodeEncode.GetString(new byte[]{temp});
            }
            return ret;
        }
    }
}
