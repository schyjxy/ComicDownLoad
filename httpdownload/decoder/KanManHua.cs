using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Collections;
using System.Text.RegularExpressions;

namespace comicDownLoad
{
    class KanManHua : PublicThing
    {
        string inpuText = null;
        string host = "https://www.manhuagui.com/";
        string homePage = "https://www.manhuagui.com";
        string comicAttach = "https://i.hamreus.com";
        string keyBase64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

        public string m_response;

        string[] arryData = new string[]
         {
             "0", "1", "2" ,"3", "4", "5",
             "6", "7", "8", "9", "a", "b",
             "c", "d", "e", "f", "g", "h",
             "i", "j", "k", "l", "m", "n",
             "o", "p", "q", "r", "s", "t",
             "u", "v", "w", "x", "y", "z"               
         };

        public override DownLoadComic GetDownImageList(string response)
        {
            string temp = "";
            DownLoadComic downLoad = new DownLoadComic();
            List<string> imgList = new List<string>();
            var ret = DecodeURL(response);
            ret = ret.Substring(ret.IndexOf("{"), ret.LastIndexOf("}") - ret.IndexOf("{") + 1);
            KanManHuaInfo info = JsonOperate.DeserializeJsonToObject<KanManHuaInfo>(ret);

            foreach (var i in info.files)
            {
                temp = comicAttach + info.path + i + "?cid=" + info.cid + "&md5=" + info.sl.md5;
                imgList.Add(temp);
            }

            downLoad.ImageList = imgList;
            downLoad.Count = imgList.Count;
            return downLoad;
        }

        public override SearchResult GetSearchComic(string response)
        {
            BasicComicInfo comicInfo;
            SearchResult result = new SearchResult();
            Queue<BasicComicInfo> queue = new Queue<BasicComicInfo>();
            Regex[] regex = new Regex[2];
            regex[0] = new Regex(@"找到\s*(<\w+>)?(?<count>\d+)");
            regex[1] = new Regex(@"<a\sclass=""bcover""\s*href=""(?<href>[\w/_-]*)""\s*title=""(?<title>[\：\w\s\[\]\！\。\、\-\—\~\（\）\！]*)"">\s*<img\s*src=""(?<url>[\w\s/:.]*)""");
            var count = regex[0].Match(response).Groups["count"].Value;
            result.Count = Convert.ToInt32(count);

            foreach (Match match in regex[1].Matches(response))
            {
                comicInfo = new BasicComicInfo();
                comicInfo.ComicHref = homePage + match.Groups["href"].Value;
                comicInfo.ComicImgUrl = match.Groups["url"].Value;
                comicInfo.ComicName = match.Groups["title"].Value;
                queue.Enqueue(comicInfo);
            }

            result.SearchQueue = queue;
            return result;
        }

        public override ComicInfo GetComicInfo(string response)//
        {
            int len = 0;
            ComicInfo info;
            HtmlDocument doc;
            info = new ComicInfo();
            doc = new HtmlDocument();
            doc.LoadHtml(response);
            HtmlNodeCollection collect = doc.DocumentNode.SelectNodes("//ul[@class='detail-list cf']/li");
            len = collect[1].SelectNodes("./span/a").Count-1;
            info.Author = collect[1].SelectNodes("./span/a")[len].Attributes["title"].Value;
            info.otherWork = homePage + collect[1].SelectNodes("./span/a")[len].Attributes["href"].Value;
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//li[@class='status']");
            info.HasFinished = node.SelectNodes("./span/span")[0].InnerText;
            info.Tag = collect[1].SelectNodes("./span/a")[0].InnerText;
            info.Description = doc.DocumentNode.SelectSingleNode("//div[@id='intro-cut']").InnerText;
            collect = doc.DocumentNode.SelectNodes("//div[@class='chapter-list cf mt10']/ul/li/a");
            Dictionary<string,string> dict;
            dict = new Dictionary<string,string>();

            foreach (HtmlNode temp in collect)
            {
                 if(dict.ContainsKey(temp.Attributes["title"].Value) == false)
                 {
                    dict.Add(temp.Attributes["title"].Value, homePage + temp.Attributes["href"].Value);
                 }
            }

            info.URLDictionary = dict;
            return info;
        }

        public override CategoryInfo GiveCategoryInfo(string response)//获取漫画分类列表
        {
            HtmlDocument doc;
            CategoryInfo info;
            doc = new HtmlDocument();
            info = new CategoryInfo();
            Dictionary<string, string> dict;
            dict = new Dictionary<string, string>();
            doc.LoadHtml(response);
            HtmlNodeCollection collect = doc.DocumentNode.SelectNodes("//div[@class='filter genre']/ul/li/a");
            
            foreach (HtmlNode node in collect)
            {
                if (dict.ContainsKey(node.InnerText) == false)
                {
                    dict.Add(node.InnerText, homePage + node.Attributes["href"].Value);
                }
            }

            info.ComicList = dict;
            return info;
        }

        public override CategoryCollect FindComicByCategory(string cateGoryStr)//通过漫画分类获取漫画
        {
            var basicInfo = new BasicComicInfo();
            var comicCollect = new CategoryCollect();
            var bookList = AnalyseTool.GetTag(cateGoryStr, @"<div class=""book-list"">", "</div>");
            comicCollect.Count = 0;

            Regex regex = new Regex(@"href=""(?<href>[/\w]*)""\stitle=""(?<title>[\w\&！!，s]*)""><img\s*(data-src|src)=""(?<url>[:\w./]*)""");
            var comicQueue = new Queue<BasicComicInfo>();

            foreach (Match i in regex.Matches(bookList))
            {
                basicInfo = new BasicComicInfo();
                basicInfo.ComicHref = host + i.Groups["href"].Value;
                basicInfo.ComicName = i.Groups["title"].Value;
                basicInfo.ComicImgUrl = i.Groups["url"].Value;
                comicQueue.Enqueue(basicInfo);
            }


            var pageInfo = AnalyseTool.GetTag(cateGoryStr, @"<div class=""pager-cont"">", "</div>");
            regex = new Regex(@"href=""(?<href>[/\w._]*)""\s*\w+=""[\w-:;]*"">下一页");
            comicCollect.NextPageUrl = homePage + regex.Match(cateGoryStr).Groups["href"].Value;
            comicCollect.Count = comicQueue.Count;
            comicCollect.ComicQueue = comicQueue;
            return comicCollect;
        }

        public override Queue<BasicComicInfo> GetTopComic(string response)
        {
            Queue<BasicComicInfo> comicQueue;
            BasicComicInfo basic;
            comicQueue = new Queue<BasicComicInfo>();
            response = AnalyseTool.GetTag(response, @"<div class=""cmt-cont cf""", "</div>");
            var pattern = @"href=""(?<href>[/\w]*)""\s*title=""(?<title>[\w\s;！♀♂&【】&]*)""><img\s*(src|data-src)=""(?<url>[:\w/.]*)""";
            Regex regex = new Regex(pattern);
            MatchCollection matchCollect = regex.Matches(response);

            var count = 0;

            foreach (Match i in matchCollect)
            {
                basic = new BasicComicInfo();
                basic.ComicName = i.Groups["title"].Value;
                basic.ComicImgUrl = i.Groups["url"].Value;
                basic.ComicHref = host + i.Groups["href"].Value;
                comicQueue.Enqueue(basic);
                count++;
            }

            return comicQueue;
        }

        class Data
        {
            public int val;
            public int position;
            public int index;

            public Data()
            {
                index = 1;
            }
        }

        class DicString
        {
            Hashtable tab1;
            Hashtable tab2;

            public DicString()
            {
                tab1 = new Hashtable();
                tab2 = new Hashtable();
            }

            public void Add(string index1, string index2, int val)
            {

                if (tab2.ContainsKey(index1) == false)
                {
                    tab1.Add(index2, val);//第二步
                    tab2.Add(index1, tab1);//第一步

                }
                else
                {
                    if (tab1.ContainsKey(index2))
                        tab1[index2] = val;
                    else
                        tab1.Add(index2, val);
                    tab2[index1] = tab1;

                }

            }


            public int GetValue(string index1, string index2)
            {
                Hashtable temp = (Hashtable)tab2[index1];
                return Convert.ToInt32(temp[index2]);
            }
        }

        public string DecTo36(int val)
        {
            int temp1 = val;
            int temp = 0;
            string result = "";
            List<string> save;
            save = new List<string>();

            if (val < 36)
            {
                return arryData[val];
            }

            while (temp1 > 0)
            {
                temp = temp1 % 36;
                save.Add(arryData[temp]);
                temp1 = temp1 / 36;
            }

            for (int i = save.Count - 1; i >= 0; i--)
            {
                result = result + save[i];
            }

            return result;
        }

        private int SearchInArry(string[] data, string find)
        {
            int pos = 0;
            foreach (var i in data)
            {
                if (i == find)
                    break;
                else
                    pos++;
            }
            return pos;

        }

        private string ThreeeToDec(string dat)
        {
            int sum = 0;
            int rate = 0;

            if (dat.Length < 2)
                return SearchInArry(arryData, dat).ToString();

            for (int i = dat.Length - 1; i >= 0; i--)
            {
                var temp = SearchInArry(arryData, dat[i].ToString());
                var min = temp * Math.Pow(36, rate++);
                sum = sum + Convert.ToInt32(min);
            }

            return sum.ToString();
        }


        private int getBaseValue(string alphabet, string character)
        {
            DicString dic = new DicString();

            for (int i = 0; i < alphabet.Length; i++)
                dic.Add(alphabet, alphabet[i].ToString(), i);

            var cdata = dic.GetValue(alphabet, character);
            return cdata;
        }

        public string eFun(int c, int a)
        {
            return (c < a ? "" : eFun(c / a, a)) + ((c = c % a) > 35 ? Convert.ToChar(c + 29).ToString() : DecTo36(c));
        }

        private string Decode_0(int len, int resetValue, int temp)
        {
            int next;
            List<object> dictionary;
            dictionary = new List<object>();
            string result = "";
            var enlargeIn = 4;
            var dictSize = 4;
            var numBits = 3;
            var entry = "";
            Data data = new Data();
            data.val = GetNextValue(0);
            data.position = resetValue;

            for (int i = 0; i < 3; i++)
            {
                dictionary.Add(i);
            }

            dictionary.Add("");
            var bits = 0;
            var maxPower = Math.Pow(2, 2);
            var power = 1;
            int resb;

            while (power != maxPower)
            {
                resb = data.val & data.position;
                data.position >>= 1;

                if (data.position == 0)
                {
                    data.position = resetValue;
                    data.val = GetNextValue(data.index++);
                }
                bits |= (resb > 0 ? 1 : 0) * power;
                power <<= 1;
            }

            object c = null;

            switch (next = bits)
            {
                case 0:
                    bits = 0;
                    maxPower = Math.Pow(2, 8);
                    power = 1;
                    while (power != maxPower)
                    {
                        resb = data.val & data.position;
                        data.position >>= 1;

                        if (data.position == 0)
                        {
                            data.position = resetValue;
                            data.val = GetNextValue(data.index++);
                        }
                        bits |= (resb > 0 ? 1 : 0) * power;
                        power <<= 1;

                    }

                    c = Convert.ToChar(bits).ToString();
                    break;

                case 1:
                    bits = 0;
                    maxPower = Math.Pow(2, 16);
                    power = 1;
                    while (power != maxPower)
                    {
                        resb = data.val & data.position;
                        data.position >>= 1;
                        if (data.position == 0)
                        {
                            data.position = resetValue;
                            data.val = GetNextValue(data.index++);
                        }
                        bits |= (resb > 0 ? 1 : 0) * power;
                        power <<= 1;
                    }
                    c = Convert.ToChar(bits);
                    break;

                case 2: return "";
            }

            dictionary[3] = c;
            string w = c.ToString();
            result += c;

            while (true)
            {
                if (data.index > len)
                {
                    return "";
                }

                bits = 0;
                maxPower = Math.Pow(2, numBits);
                power = 1;

                while (power != maxPower)
                {
                    resb = data.val & data.position;
                    data.position >>= 1;

                    if (data.position == 0)
                    {
                        data.position = resetValue;
                        data.val = GetNextValue(data.index++);
                    }

                    bits |= (resb > 0 ? 1 : 0) * power;
                    power <<= 1;
                }

                int c1;//第二次判断bit，此时C为整形
                switch (c1 = bits)
                {
                    case 0:
                        bits = 0;
                        maxPower = Math.Pow(2, 8);
                        power = 1;

                        while (power != maxPower)
                        {
                            resb = data.val & data.position;
                            data.position >>= 1;
                            if (data.position == 0)
                            {
                                data.position = resetValue;
                                data.val = GetNextValue(data.index++);
                            }
                            bits |= (resb > 0 ? 1 : 0) * power;
                            power <<= 1;
                        }

                        c = Convert.ToChar(bits);

                        if (dictionary.Count > dictSize)
                            dictionary[dictSize++] = c;
                        else
                        {
                            dictionary.Add(c);
                            dictSize++;
                        }

                        c1 = dictSize - 1;
                        enlargeIn--;
                        break;
                    case 1:
                        bits = 0;
                        maxPower = Math.Pow(2, 16);
                        power = 1;
                        while (power != maxPower)
                        {
                            resb = data.val & data.position;
                            data.position >>= 1;

                            if (data.position == 0)
                            {
                                data.position = resetValue;
                                data.val = GetNextValue(data.index++);
                            }
                            bits |= (resb > 0 ? 1 : 0) * power;
                            power <<= 1;
                        }

                        c = Convert.ToChar(bits);
                        if (dictionary.Count > dictSize)
                            dictionary[dictSize++] = c;
                        else
                        {
                            dictionary.Add(c);
                            dictSize++;
                        }
                        c1 = dictSize - 1;
                        enlargeIn--;
                        break;
                    case 2:
                        return result;////需要添加函数实现
                }

                if (enlargeIn == 0)
                {
                    enlargeIn = Convert.ToInt32(Math.Pow(2, numBits));
                    numBits++;
                }

                if (dictionary.Count > c1)
                {
                    entry = dictionary[c1].ToString();
                }
                else
                {
                    if (c1 == dictSize)
                    {
                        // entry = w + entry[0].ToString();//返回指定位置的字符，这个语句导致BUG，这句已经修改
                        entry = w + w[0].ToString();

                    }
                    else
                        return null;
                }

                result += entry;

                //首先判断是不是存在该列
                if (dictionary.Count > dictSize)//存在该列
                {
                    dictionary[dictSize++] = w + entry[0].ToString();
                }
                else
                {
                    dictionary.Add(w + entry[0].ToString());
                    dictSize++;
                }

                enlargeIn--;
                w = entry;

                if (enlargeIn == 0)
                {
                    enlargeIn = Convert.ToInt32(Math.Pow(2, numBits));
                    numBits++;
                }
            }
        }

        private int GetNextValue(int index)
        {
            int result;
            result = getBaseValue(keyBase64, inpuText[index].ToString());
            return result;
        }

        public string DecodeFormBase64(string input)
        {

            if (input == null || input == "")
                return "输入为空";

            inpuText = input;

            return Decode_0(input.Length, 32, GetNextValue(0));
        }

        private string OrOperate(string a, string b)
        {
            if (a != "")
                return a;
            else
                return b;
        }

        public string ExcuteFileInfo(int numA, int numC, string sourceStr, string[] replaceStr)
        {
            string temp = "";
            var dat = new Dictionary<object, string>();

            while (numC-- > 0)
            {
                temp = OrOperate(replaceStr[numC], eFun(numC, numA));
                dat.Add(eFun(numC, numA), temp);
            }

            Regex regex = new Regex(@"\b\w+\b");
            MatchCollection match = regex.Matches(sourceStr);

            foreach (Match i in match)
            {
                sourceStr = Regex.Replace(sourceStr, @"\b" + i.Value + @"\b", dat[i.Value]);
            }

            return sourceStr;
        }

        public string DecodeURL(string response)//从网页html提取加密数据，
        {
            string analyseDat = "";
            string retData = "";
            string input = "";
            var a = 0;
            var c = 0;
            var e = 0;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            HtmlNodeCollection collect = doc.DocumentNode.SelectNodes("//script[@type='text/javascript']");
            foreach (HtmlNode node in collect)
            {
                if (node.InnerText.IndexOf("window") >= 0)
                {
                    analyseDat = node.InnerText;
                    break;
                }
            }

            //replaceText，提取出要被替换的字符
            Regex regex = new Regex(@"\}\((?<data>.+)");
            analyseDat = regex.Match(analyseDat).Groups["data"].Value;
            analyseDat = analyseDat.Substring(analyseDat.IndexOf("'") + 1);
            var replaceText = analyseDat.Substring(0, analyseDat.IndexOf(@"'"));
            //下面是截取
            analyseDat = analyseDat.Substring(analyseDat.IndexOf(@"'") + 1);

            regex = new Regex(@"(?<data>\d+)\,");
            MatchCollection macthCollect = regex.Matches(analyseDat);
            a = Convert.ToInt32(macthCollect[0].Groups["data"].Value);
            c = Convert.ToInt32(macthCollect[1].Groups["data"].Value);
            e = Convert.ToInt32(macthCollect[2].Groups["data"].Value);

            analyseDat = analyseDat.Substring(analyseDat.IndexOf("'") + 1);
            analyseDat = analyseDat.Substring(0, analyseDat.IndexOf("'"));
            input = analyseDat;
            var result = DecodeFormBase64(input);
            string[] data = result.Split('|');
            retData = ExcuteFileInfo(a, c, replaceText, data);
            return retData;
        }

    }
}
