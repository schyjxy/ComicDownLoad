using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace comicDownLoad
{
    class JsonOperate
    {
        public static string SerializeObject(object o)//将对象序列化成JSON字符串
        {
            string json = JsonConvert.SerializeObject(o);
            return json;
        }

        public static T DeserializeJsonToObject<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
            T t = o as T;
            return t;
        }

        public static List<T> DeserializeJsonToList<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            List<T> list = o as List<T>;
            return list;
        }
    }


    public class KanManHuaInfo
    {
        public int bid { get; set; }
        public string bname { get; set; }
        public string bpic { get; set; }
        public string cid { get; set; }
        public string cname { get; set; }
        public string[] files { get; set; }
        public bool fininshed { get; set; }
        public int len { get; set; }
        public string path { get; set; }
        public int status { get; set; }
        public string block_CC { get; set; }
        public int nextId { get; set; }
        public int preId { get; set; }
        public Sl sl { get; set; }
    }

    public class Sl
    {
        public string md5 { get; set; }
    }


}
