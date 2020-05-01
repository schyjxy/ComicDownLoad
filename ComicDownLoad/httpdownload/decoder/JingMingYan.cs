using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace comicDownLoad
{
    class JingMingYan : PublicThing
    {
        public string hostName = "http://www.jmydm.com";
       
        public Queue<BasicComicInfo> AnalyseURL(string response, string express)
        {
            Regex regex = new Regex(express);
            BasicComicInfo basic = new BasicComicInfo();
            Queue<BasicComicInfo> queue = new Queue<BasicComicInfo>();
            MatchCollection match = regex.Matches(response);

            foreach (Match i in match)
            {
                basic = new BasicComicInfo();
                basic.ComicImgUrl = i.Groups["url"].Value;
                basic.ComicName = i.Groups["data1"].Value + i.Groups["data2"].Value + i.Groups["data3"].Value;
                basic.ComicHref = hostName + i.Groups["href"].Value;
                queue.Enqueue(basic);
            }

            return queue;
        }

        public override ComicInfo GetComicInfo(string response)
        {
            ComicInfo comicInfo = new ComicInfo();
            Dictionary<string, string> dict;
            dict = new Dictionary<string, string>();
            var comicStr = AnalyseTool.GetTag(response, @"<div class=""cShuoMing"">", "</div>");
            var descrtion = AnalyseTool.GetTag(response, @"<div class=""cInfoArea"">", "</div>");
            descrtion = AnalyseTool.ReplacePunctuation(descrtion);
            Regex[] regex = new Regex[6];
            regex[0] = new Regex(@"""cInfoArea"">\s*(?<descr>[\w+“”，。！]*)");
            regex[1] = new Regex(@"<span\sclass=""cInfoLianWan"">(?<status>[\w\s]*)");//连载状态
            regex[2] = new Regex(@"<a\starget=_blank\s*href='[?./\w=%]*'>(?<author>\w+)</a></td");//作者及相关作品
            regex[3] = new Regex(@"<a\s*href='[/\w-]*'>(?<tag>\w+)</a>");//漫画标签
            regex[4] = new Regex(@"<a\s*target='_blank'\s*href='(?<href>[\w/]*)'>(?<title>[\w\s（）.]*)");
            regex[5] = new Regex(@"img\s*src='(?<url>[\w:/._]*)");

            comicInfo.Description = regex[0].Match(descrtion).Groups["descr"].Value;
            comicInfo.HasFinished = regex[1].Match(comicStr).Groups["status"].Value.Replace("\r\n", "").Replace(" ","");
            comicInfo.Author = regex[2].Match(comicStr).Groups["author"].Value;
            comicInfo.Tag = regex[3].Match(comicStr).Groups["tag"].Value;
            comicInfo.Year = "略";

            MatchCollection collect = regex[4].Matches(response);

            foreach (Match m in collect)
            {
                if (dict.ContainsKey(m.Groups["title"].Value) == false)
                { 
                    dict.Add(m.Groups["title"].Value, hostName + m.Groups["href"].Value);
                }
            }
            
            comicInfo.URLDictionary = dict;
            comicInfo.CoverImgUrl = regex[5].Match(response).Groups["url"].Value;
            return comicInfo;

        }

        public override CategoryInfo GiveCategoryInfo(string response)//获取漫画分类列表，一个键值对
        {
            var dataBaseName = "task.db";
            CategoryInfo info = new CategoryInfo();
            SqlOperate operate = new SqlOperate();
            operate.CreateOrOpenDataBase(dataBaseName);
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (operate.isHasTable("jingmingyanDir") == false)
            {
                operate.CreateDirTemp("jingmingyanDir");
                Regex regex = new Regex(@"<a\s*href='(?<href>/\w+-\w+/)'>(?<title>\w+)");
                MatchCollection collect = regex.Matches(response);

                foreach (Match m in collect)
                {
                    if (dict.ContainsKey(m.Groups["title"].Value) == false)
                    {
                       dict.Add(m.Groups["title"].Value, hostName + m.Groups["href"].Value);
                       operate.InserDirectoryLink("jingmingyanDir", m.Groups["title"].Value, hostName + m.Groups["href"].Value);
                    }
                }
            }
            else
            {
                dict = operate.GetDirectory("jingmingyanDir");
            }

            operate.CloseDataBase();
            info.ComicList = dict;
            return info;
        }

        public override Queue<BasicComicInfo> GetTopComic(string response)
        {
            string title = "";
            string href = "";
            string url = "";
            BasicComicInfo basicCominfo;
            Queue<BasicComicInfo> queue = new Queue<BasicComicInfo>();           
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(response);          
            HtmlNodeCollection collect = document.DocumentNode.SelectNodes("//div[@class='cHpComic']");

            foreach (var i in collect)
            {
                basicCominfo = new BasicComicInfo();
                title = i.SelectSingleNode("./a[@target='_blank']").Attributes["title"].Value;
                href = i.SelectSingleNode("./a[@target='_blank']").Attributes["href"].Value;
                url = i.SelectSingleNode("./a/img").Attributes["src"].Value;
                basicCominfo.ComicName = title;
                basicCominfo.ComicImgUrl = url;
                basicCominfo.ComicHref = hostName + href;
                queue.Enqueue(basicCominfo);
            }

            return queue;
        }

        public override DownLoadComic GetDownImageList(string response)//给出某一话所有下载链接
        {
            DownLoadComic comic = new DownLoadComic();

            try
            {
                Regex[] regex = new Regex[2];
                regex[0] = new Regex(@"(sFiles|lf\d+)=""(?<key>\w+)""");
                regex[1] = new Regex(@"sPath=""(?<path>[\w/]*)""");
                var host = "http://comic.jmydm.com:8080/";
                var sFile = regex[0].Match(response).Groups["key"].Value;
                var sPath = regex[1].Match(response).Groups["path"].Value;
                var urlList = new List<string>();
                var sk = "kxnelimwzsb";
                var dat = unsuan(sFile, sk);
                string[] file = dat.Split('|');

                foreach (var i in file)
                {
                    urlList.Add(host + sPath + i);
                }

                comic.Count = urlList.Count;
                comic.ImageList = urlList;
            }
            catch(Exception ex)
            {

            }
            
            return comic;
        }

        public override CategoryCollect FindComicByCategory(string cateGoryStr)
        {
            CategoryCollect cateCollect = new CategoryCollect();
            var dat = AnalyseTool.GetTag(cateGoryStr, @"<div class=""cComicList""", @"<div class=""cComicPageChange2");
            Regex regex = new Regex(@"href=['""](?<href>[/\w-]*)['""]\s*class=['""][\w_]*['""]\stitle=['""](?<title>[\w-！!\s，。]*)['""]><img\salt=['""][\w-!,。\s,]*['""]\s*src=['""](?<url>[\w:/.-]*)");
            var basicInfo = new BasicComicInfo();
            var comicQueue = new Queue<BasicComicInfo>();

            foreach (Match i in regex.Matches(dat))
            {
                basicInfo = new BasicComicInfo();
                basicInfo.ComicHref = hostName + i.Groups["href"].Value;
                basicInfo.ComicName = i.Groups["title"].Value;
                basicInfo.ComicImgUrl = i.Groups["url"].Value;
                comicQueue.Enqueue(basicInfo);
            }

            dat = AnalyseTool.GetTag(cateGoryStr, "<span class='cPageChangeLink'>", "</span>");
            regex = new Regex(@"<a\s*href='(?<url>[\w-/]*)'>下一页");
            cateCollect.NextPageUrl = hostName + regex.Match(dat).Groups["url"].Value;
            cateCollect.ComicQueue = comicQueue;
            cateCollect.PagesCollection = new Dictionary<string, string>();
            return cateCollect;
        }

        public override SearchResult GetSearchComic(string response)//给出搜索漫画结果
        {
            BasicComicInfo comicInfo;
            SearchResult result = new SearchResult();
            Queue<BasicComicInfo> queue = new Queue<BasicComicInfo>();
            Regex[] regex = new Regex[3];
            regex[0] = new Regex(@"(?<count>\d+)</\w+>\s*本相关漫画");
            regex[1] = new Regex(@"img\salt='(?<title>[\w\s，。！]*)?(<font\s*color=red>)?\s*(?<title1>[\w\s，。！]*)?(</font>)?(?<title2>\w+)?'\s*src='(?<url>[\w./:-]*)'></div>\s*<div\s*class='cListTitle'>\s*<a\s*target='_blank'\s*href='(?<href>[\w-/]*)'");
            regex[2] = new Regex(@"");//下一页
          
            foreach (Match macth in regex[1].Matches(response))
            {
                comicInfo = new BasicComicInfo();
                comicInfo.ComicHref = hostName + macth.Groups["href"].Value;
                comicInfo.ComicName = macth.Groups["title"].Value + macth.Groups["title1"].Value + macth.Groups["title2"].Value;
                comicInfo.ComicImgUrl = macth.Groups["url"].Value;
                queue.Enqueue(comicInfo);
            }

            var count = regex[0].Match(response).Groups["count"].Value;

            if(count != "")
                 result.Count = Convert.ToInt32(count);
            else
                result.Count = 0;

            result.SearchQueue = queue;
            return result;
        }

        public static string unsuan(string s, string a)//输入参数,s为页面上的sFiles,a为页面上的sFilePath
        {
            var k = a.Substring(0, a.Length-1);
            var f = a.Substring(a.Length-1);

            for (int i = 0; i < k.Length; i++)
            {
                s = s.Replace(k.Substring(i, 1), i.ToString());
            }

            System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
            byte[] byteArray = null;
            var save = s.Split(new char[] { f.ToCharArray()[0] });
            s = "";
            foreach (var i in save)
            {
                byteArray = new byte[] { (byte)Convert.ToInt32(i) };
                s = s + asciiEncoding.GetString(byteArray);
            }

            return s;
        }
    }

}
