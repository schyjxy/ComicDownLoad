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
        private static extern int GetPrivateProfileString(string sectionName, string key, string defaultValue, byte[] returnBuffer, int size, string filePath);
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string sectionName, string key, string value, string filePath);

        private static string m_savePath;
        private static string fileName = "CONGFIG.INI";

        public string SavePath { get { return m_savePath; } set { m_savePath = value; } }

        public static void  SaveConfig(string key, string value)
        {
            string section = "";

            switch (key)
            {
                case "downPath": section = "DownLoadPath"; break;
                
            }

            WritePrivateProfileString(section, key, value, m_savePath + fileName);
        }
    }
}
