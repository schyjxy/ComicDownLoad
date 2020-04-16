using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace comicDownLoad
{
    class ComicConfig
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        private static string m_savePath;
        private static string fileName = ".\\CONFIG.INI";

        public string SavePath { get { return m_savePath; } set { m_savePath = value; } }

        public static void SaveConfig(string key, string value)
        {
            string section = "";

            switch (key)
            {
                case "savePath": section = "Config"; break;
                case "proxyIP": section = "Config"; break;
                case "proxyPort": section = "Config"; break;
            }

            WritePrivateProfileString(section, key, value, fileName);
        }

        public static string ReadConfig(string key)
        {
            string section = "";
            StringBuilder result = new StringBuilder(500);

            switch (key)
            {
                case "savePath": section = "Config"; break;
                case "proxyIP": section = "Config"; break;
                case "proxyPort": section = "Config"; break;
            }

            GetPrivateProfileString(section, key, "", result, 500, fileName);
            return result.ToString();
        }
    }
}
