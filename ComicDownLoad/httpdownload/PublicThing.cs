using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace comicDownLoad
{

    public class ComicInfo//反应漫画的信息
    {
        public string CoverImgUrl { get; set; }
        public string Year { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string otherWork { get; set; }//作者其他作品
        public string Tag { get; set; }//所属标签
        public string Hot { get; set; }//热度
        public string Comment { get; set; }//可有可无 
        public string HasFinished { get; set; }//是否完结
        public Dictionary<string, string> URLDictionary { get; set; }
    }


    public class BasicComicInfo//最简单的漫画信息
    {
        public string ComicName{get;set;}
        public string ComicHref{get;set;}
        public string ComicImgUrl{get;set;}
        public string UpdateStatus { get; set; }
    }

    public class CategoryCollect//记录一堆漫画信息
    {
        public Queue<BasicComicInfo> ComicQueue;
        public int Count { get; set; }
        public int ComicTotalCount { get; set; }
        public string NextPageUrl { get; set; }
        public string LastPageUrl { get; set; }
        public Dictionary<string, string> PagesCollection { get; set; }
    }

    public class DownLoadComic//下载的漫画集合
    {
        public string ComicName { get; set; }//下载的漫画名
        public int Count { get; set; }//下载的漫画个数
        public List<string> ImageList { get; set; }
    }

    public class CategoryInfo//
    {
        public Dictionary<string, string> ComicList{get;set;}   
    }

    public class SearchResult
    {
        public int Count { get; set; }
        public Queue<BasicComicInfo> SearchQueue { get; set; }
        public string NextPageUrl { get; set; }
        public string LastPageUrl { get; set; }
    }

    public class PublicThing//公共接口
    {
        public string BackUrl { get; set; }
        public string currentUrl = "";

        public PublicThing()
        {

        }

        public virtual CategoryCollect FindComicByCategory(string cateGoryStr)
        {
            return new CategoryCollect();
        }

        protected HtmlNode GetMainNode(string data)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(data);
            return doc.DocumentNode;
        }

        public virtual CategoryInfo GiveCategoryInfo(string response)//给出漫画列表
        {
            CategoryInfo cateInfo = new CategoryInfo();

            return cateInfo;
        }


        public virtual Queue<BasicComicInfo> GetTopComic(string response)//获取热门漫画
        {
            BasicComicInfo comicInfo;
            comicInfo = new BasicComicInfo();
            Queue<BasicComicInfo> queue = new Queue<BasicComicInfo>();
            return queue;
        }

        public virtual void AnalyseHomePage(string message)//分析主页
        { 
            
        }

        public virtual ComicInfo GetComicInfo(string response)//获取漫画相关信息
        {
            ComicInfo comicIno = new ComicInfo();
            return comicIno;
        }

        public virtual DownLoadComic GetDownImageList(string response)//获取下载的漫画集合
        {
            DownLoadComic downComic = new DownLoadComic();           
            return downComic;
        }

        public virtual SearchResult GetSearchComic(string response)//获取搜索结果
        {
            SearchResult searchResult = new SearchResult();
            return searchResult;
        }

        public virtual Dictionary<string, string> GetLatestUpdate()//获取最新漫画
        {
            return new Dictionary<string, string>();
        }
    }
}
