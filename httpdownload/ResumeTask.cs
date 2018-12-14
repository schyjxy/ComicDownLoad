using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace comicDownLoad
{
    public class ResumeTask//记录未下载完成的
    {
        public string ComicDic { get; set; }//漫画名字
        public string ComicUrl { get; set; }//漫画连接
        public string HasDownLoad { get; set; }//已经下载的进度
    }
}
