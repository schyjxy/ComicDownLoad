using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace comicDownLoad
{
    class HuHuManHua:PublicThing
    {
        string hostName = "http://www.huhudm.com/";    

        public override Queue<BasicComicInfo> GetTopComic(string response)
        {
            Queue<BasicComicInfo> queue;
            queue = new Queue<BasicComicInfo>();
            HtmlNode mainNode = GetMainNode(response);
            
            if (mainNode == null || response == "")
            {
                return queue;
            }

            BasicComicInfo comicInfo;
            HtmlNodeCollection collect = mainNode.SelectNodes("//div[@class='cTabHotHtmHide']/li");
            
            foreach (HtmlNode nodeTemp in collect)
            {
                comicInfo = new BasicComicInfo();
                comicInfo.ComicHref =  hostName + nodeTemp.SelectSingleNode("./a").Attributes["href"].Value;
                comicInfo.ComicName = nodeTemp.SelectSingleNode("./a").Attributes["title"].Value;
                comicInfo.ComicImgUrl = nodeTemp.SelectSingleNode("./a/img").Attributes["src"].Value;
                queue.Enqueue(comicInfo);
            }

            return queue;
        }

        public override CategoryCollect FindComicByCategory(string cateGoryStr)
        {
            CategoryCollect retCollect = new CategoryCollect();
            var mainNode = GetMainNode(cateGoryStr);
            
            if (mainNode == null)
            {
                return retCollect;
            }

            BasicComicInfo info;
            Queue<BasicComicInfo> queue = new Queue<BasicComicInfo>();
            HtmlNodeCollection collect = mainNode.SelectNodes("//div[@class='cComicList']/li");
            
            foreach (HtmlNode nodeTemp in collect)
            {
                info = new BasicComicInfo();
                info.ComicHref = hostName + nodeTemp.SelectSingleNode("./a").Attributes["href"].Value;
                info.ComicName = nodeTemp.SelectSingleNode("./a").Attributes["title"].Value;
                info.ComicImgUrl = nodeTemp.SelectSingleNode("./a/img").Attributes["src"].Value;
                queue.Enqueue(info);
            }

            collect = mainNode.SelectNodes("//span[@class='cPageChangeLink']/a");
            
            if (collect != null)
            {
                string key;
                Dictionary<string, string> dict;
                dict = new Dictionary<string, string>();

                foreach (var i in collect)
                {
                    key = i.InnerText;

                    if (dict.ContainsKey(key) == false && i.Attributes["href"] != null)
                    {
                        dict.Add(key, hostName + i.Attributes["href"].Value);
                    }
                }

                if (dict.ContainsKey("上一页"))
                {
                    retCollect.LastPageUrl = dict["上一页"];
                }

                if (dict.ContainsKey("下一页"))
                {
                    retCollect.NextPageUrl = dict["下一页"];
                }
            }

            HtmlNode node = mainNode.SelectSingleNode("//div[@class='cComicPageChange']");

            if (node != null)
            {
                string temp = node.SelectNodes("./b")[0].InnerText;
                retCollect.Count = Convert.ToInt32(temp);
            }

            retCollect.ComicQueue = queue;
           
            return retCollect;
        }

        public override CategoryInfo GiveCategoryInfo(string response)
        {
            CategoryInfo info = new CategoryInfo();
            HtmlNode mainNode = GetMainNode(response);
            HtmlNode node = mainNode.SelectSingleNode("//div[@class='cHNav']/div");
            HtmlNodeCollection collect = node.SelectNodes("./span/a");

            string key;
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("全部", "http://www.huhudm.com/comic/");

            if(collect != null)
            {
                foreach (HtmlNode nodeTemp in collect)
                {
                    key = nodeTemp.InnerText;
                
                    if (dict.ContainsKey(key) == false)
                    {
                        dict.Add(key, hostName + nodeTemp.Attributes["href"].Value);
                    }
                }
            }
            
            info.ComicList = dict;
            return info;
        }

        public override ComicInfo GetComicInfo(string response)
        {
            ComicInfo comicInfo;
            comicInfo = new ComicInfo();
            HtmlNode mainNode = GetMainNode(response);
            
            if (mainNode == null)
            {
                return comicInfo;
            }

            HtmlNode node = mainNode.SelectSingleNode("//div[@id='about_kit']");
            HtmlNodeCollection collect = node.SelectNodes("./ul/li");
            
            if (collect != null)
            {
                comicInfo.Author = collect[1].InnerText.Replace("作者:", ""); ;
                comicInfo.HasFinished = collect[2].InnerText.Replace("状态:", "");
                comicInfo.Description = collect[collect.Count - 1].InnerText.Replace("简介:", "");
                comicInfo.Tag = "无";
            }
          
            collect = mainNode.SelectNodes("//ul[@class='cVolUl']/li/a");

            if (collect != null)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                string key = "";

                foreach (HtmlNode nodeTemp in collect)
                {
                    key = nodeTemp.Attributes["title"].Value;

                    if (dict.ContainsKey(key) == false)
                    {
                        dict.Add(key, hostName + nodeTemp.Attributes["href"].Value);
                    }
                }

                comicInfo.URLDictionary = dict;
            }

            return comicInfo;
        }

        public override DownLoadComic GetDownImageList(string response)
        {
            HanHanManhua hanhan = new HanHanManhua();
            var comic = hanhan.GetDownImageList(response);
            return comic;
        }
    }
}
