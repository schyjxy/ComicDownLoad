using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace comicDownLoad
{
    class Resource
    {
        private Dictionary<string, string> comicDictionary;
        private Dictionary<string, string> searchResult;

        public string ComicName { get; set; }
        public string ComicHref { get; set; }

        public Dictionary<string, string> SearchResultURL
        {
            get
            { return searchResult; }
            set
            { searchResult = value; }
        }

        public Dictionary<string, string> CategoryCollect
        {
            get;
            set;
        }

        public Dictionary<string, string> ComicDictionary
        {
            get { return comicDictionary; }
            set { comicDictionary = value; }
        }

        public string NextPage { get; set; }
        public string LastPage { get; set; }
    }

    
}
