using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace comicDownLoad
{
    class DecoderDistrution
    {
        private static PublicThing publicdecoder;

        public static PublicThing GiveDecoder(string url)//响应的实例化
        {
            Uri uri = new Uri(url);
            var index = uri.Host.IndexOf(".") + 1;
            var host = uri.Host.Substring(index);

            switch(host)
            {
                case "manhuagui.com": publicdecoder = new KanManHua(); break;
                case "jmydm.com": publicdecoder = new JingMingYan(); break;
                case "hhimm.com": publicdecoder = new HanHanManhua(); break;
                case "se": publicdecoder = new YYSLManHua(); break;
                case "dmzx.com": publicdecoder = new DongManZaiXian(); break;
                case "k886.net": publicdecoder = new KanmanhuaK886(); break;
                case "verydm.com": publicdecoder = new VeryDm(); break;
                case "1kkk.com": publicdecoder = new ManHuaRen(); break;
            }
            return publicdecoder;
        }
    }
}
