using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Security;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace comicDownLoad
{
    class ManHuaDui : PublicThing
    {
        string hostName = "https://www.manhuadui.com";

        public override Queue<BasicComicInfo> GetTopComic(string response)
        {
            Queue<BasicComicInfo> queue = new Queue<BasicComicInfo>();
            HtmlNode mainNode = GetMainNode(response);
            HtmlNode node = mainNode.SelectSingleNode("//ul[@class='list_con_li clearfix']");
            HtmlNodeCollection collect = node.SelectNodes("./li");
            
            if (collect == null)
            {
                return queue;
            }

            BasicComicInfo comicInfo;

            foreach (HtmlNode nodeTemp in collect)
            {
                comicInfo = new BasicComicInfo();
                comicInfo.ComicHref = nodeTemp.SelectSingleNode("./a").Attributes["href"].Value;
                comicInfo.ComicName = nodeTemp.SelectSingleNode("./a/img").Attributes["alt"].Value;
                comicInfo.ComicImgUrl = nodeTemp.SelectSingleNode("./a/img").Attributes["src"].Value;
                queue.Enqueue(comicInfo);
            }
                      
            return queue;
        }

        public override ComicInfo GetComicInfo(string response)
        {
            string desc = "";
            ComicInfo comicInfo = new ComicInfo();
            HtmlNode mainNode = GetMainNode(response);
            HtmlNode node = mainNode.SelectSingleNode("//ul[@class='comic_deCon_liO']");
            HtmlNodeCollection collect = node.SelectNodes("./li/a");
            

            if (collect != null)
            {
                comicInfo.Author = collect[0].InnerText;
                comicInfo.HasFinished = collect[1].InnerText;
                comicInfo.Tag = collect[2].InnerText;
                comicInfo.otherWork = hostName + collect[0].Attributes["href"].Value;
                desc = mainNode.SelectSingleNode("//p[@class='comic_deCon_d']").InnerText.Trim();
                comicInfo.Description = desc.Substring(desc.IndexOf("：") + 1);
            }
           
            collect = mainNode.SelectNodes("//ul[@id='chapter-list-1']/li/a");
            
            if (collect != null)
            {
                string key = "";
                Dictionary<string, string> dict = new Dictionary<string, string>();

                foreach (HtmlNode temp in collect)
                {
                    key = temp.Attributes["title"].Value;
                    
                    if (dict.ContainsKey(key) == false)
                    {
                        dict.Add(key, hostName + temp.Attributes["href"].Value);
                    }
                }

                comicInfo.URLDictionary = dict;
            }
            
            return comicInfo;
        }

        public static string AesDecrypt(string str, string key, string iv)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Convert.FromBase64String(str);

            RijndaelManaged rm = new RijndaelManaged
            {
                IV = Encoding.UTF8.GetBytes(iv),
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            ICryptoTransform cTransform = rm.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }

        public override CategoryInfo GiveCategoryInfo(string response)
        {
            string key = "";
            HtmlNodeCollection urlCollect;
            CategoryInfo info = new CategoryInfo();
            HtmlNode mainNode = GetMainNode(response);
            HtmlNodeCollection collect = mainNode.SelectNodes("//div[@class='filter-item clearfix']");

            if (collect != null)
            {
                urlCollect = collect[2].SelectNodes("./ul/li/a");
            }
            else
            {
                return info;
            }

            if (urlCollect != null)
            {
                info.ComicList = new Dictionary<string, string>();

                foreach (HtmlNode node in urlCollect)
                {
                    key = node.InnerText;

                    if (info.ComicList.ContainsKey(key) == false)
                        info.ComicList.Add(key, hostName + node.Attributes["href"].Value);
                }
            }
            

            return info;
        }

        public override SearchResult GetSearchComic(string response)
        {
            SearchResult searchResult;
            BasicComicInfo basicComicInfo;
            searchResult = new SearchResult();
            HtmlNode mainNode = GetMainNode(response);
            Queue<BasicComicInfo> queue = new Queue<BasicComicInfo>();

            HtmlNodeCollection nodeCollect = mainNode.SelectNodes("//ul[@class='list_con_li update_con autoHeight']/li");

            if (nodeCollect != null)
            {
                foreach (HtmlNode node in nodeCollect)
                {
                    basicComicInfo = new BasicComicInfo();
                    basicComicInfo.ComicHref = node.SelectSingleNode("./a").Attributes["href"].Value;
                    basicComicInfo.ComicName = node.SelectSingleNode("./a").Attributes["title"].Value;
                    basicComicInfo.ComicImgUrl = node.SelectSingleNode("./a/img").Attributes["src"].Value;
                    queue.Enqueue(basicComicInfo);
                }
               
            }

            string key = "";
            nodeCollect = mainNode.SelectNodes("//ul[@class='pagination']/li/a");

            if (nodeCollect != null)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();

                foreach (HtmlNode temp in nodeCollect)
                {
                    key = temp.InnerText;

                    if (dict.ContainsKey(key) == false)
                    {
                        dict.Add(key, hostName + temp.Attributes["href"].Value);
                    }
                }

                if (dict.ContainsKey("下一页"))
                {
                    searchResult.NextPageUrl = dict["下一页"];
                }

            }

            searchResult.SearchQueue = queue;
            int num = Convert.ToInt32(mainNode.SelectSingleNode("//em[@class='c_6']").InnerText);
            searchResult.Count = num;
            return searchResult;
        }

        public override CategoryCollect FindComicByCategory(string cateGoryStr)
        {
            CategoryCollect cateCollect = new CategoryCollect();
            Queue<BasicComicInfo> queue = new Queue<BasicComicInfo>();
            HtmlNode mainNode = GetMainNode(cateGoryStr);
            cateCollect.ComicQueue = queue;
            HtmlNode node = mainNode.SelectSingleNode("//ul[@class='list_con_li clearfix']");

            if(node == null)
            {
                return cateCollect;
            }

            HtmlNodeCollection collect = node.SelectNodes("./li");

            if (collect == null)
            {
                return cateCollect;
            }

            BasicComicInfo comicInfo;

            foreach (HtmlNode nodeTemp in collect)
            {
                comicInfo = new BasicComicInfo();
                comicInfo.ComicHref = nodeTemp.SelectSingleNode("./a").Attributes["href"].Value;
                comicInfo.ComicName = nodeTemp.SelectSingleNode("./a/img").Attributes["alt"].Value;
                comicInfo.ComicImgUrl = nodeTemp.SelectSingleNode("./a/img").Attributes["src"].Value;
                queue.Enqueue(comicInfo);
            }

            int count = 0;
            string key = "";
            node = mainNode.SelectSingleNode("//span[@class='comi_num']/em");
            count = Convert.ToInt32(node.InnerText);
            collect = mainNode.SelectNodes("//ul[@class='pagination']/li/a");
            cateCollect.PagesCollection = new Dictionary<string, string>();

            if (collect != null)
            {
                foreach (HtmlNode temp in collect)
                {
                    key = temp.InnerText;

                    if (cateCollect.PagesCollection.ContainsKey(key) == false)
                    {
                        cateCollect.PagesCollection.Add(key, hostName + temp.Attributes["href"].Value);
                    }
                }

                if (cateCollect.PagesCollection.ContainsKey("下一页"))
                {
                    cateCollect.NextPageUrl = cateCollect.PagesCollection["下一页"];
                }
            }
          
                 
            
            return cateCollect;
        }

        public override DownLoadComic GetDownImageList(string response)
        {
            DownLoadComic comic = new DownLoadComic();
            string key = "123456781234567G";
            string iv = "ABCDEF1G34123412";
            var chapterImages = "";

            Regex regx = new Regex(@"chapterImages\s+\=\s+""(?<url>[\w/\+\-\=]*)""");
            chapterImages = regx.Match(response).Groups["url"].Value;
            string ret = AesDecrypt(chapterImages, key, iv);
            comic.ImageList = new List<string>();
            JArray arry =  JArray.Parse(ret);

            foreach (var i in arry)
            {
                comic.ImageList.Add("http://img01.eshanyao.com/showImage.php?url="+ i.ToString());
            }

            comic.Count = comic.ImageList.Count;
            return comic;
        }
    }


}
