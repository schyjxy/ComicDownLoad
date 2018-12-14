using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace comicDownLoad
{
    class SqlOperate
    {
        private SQLiteConnection m_connect;

        public struct CollectStruct
        {
            public string Name { get; set; }
            public string Href { get; set; }
            public string ImagePath { get; set; }
        }

        public SqlOperate()
        {
            CreateOrOpenDataBase("task.db");
        }

        public  SQLiteConnection CreateOrOpenDataBase(string path)//建立数据库
        {
            m_connect = new SQLiteConnection("data source=" + path);
            m_connect.Open();
            return m_connect;
        }

        public void CreateTable(string tableName)//
        {
            string sql = "create table " + tableName + " " + "(taskName vchar(100), url vchar(500), path vchar(200), pageCount INTEGER, pageDownLoad INTEGER, isFinished BOOLEAN)";
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            command.ExecuteNonQuery();//执行建立表
        }

        //插入新的数据
        public void InsertData(string tableName, string taskName, string downLoadurl, string path, int pageCount, int downLoadPage, int finished)//插入数据
        {
            string sql = "";
            sql = "insert into " + tableName + " (taskName, url, path,pageCount,pageDownLoad,isFinished)" + " values ";
            sql = sql + " (" + "'" + taskName + "'" + "," + "'" + downLoadurl + "'" + "," + "'" + path + "'" + "," + pageCount.ToString() + "," + downLoadPage.ToString() + "," + finished.ToString() + ")";
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            command.ExecuteNonQuery();
        }

        public bool AddCollect(List<CollectStruct> dataList)
        {
            bool isSuccess = true;
            string commandText = "";
            SQLiteCommand command = m_connect.CreateCommand();
            string sql = "insert into collect (name,url,imagePath) values ";
           

            try
            {
                foreach (var i in dataList)
                {
                    commandText = sql + "('" + i.Name + "', '" + i.Href + "','" + i.ImagePath + "')";
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        public List<CollectStruct> GetCollet()
        {
            CollectStruct collectInfo;
            List<CollectStruct> list;
            list = new List<CollectStruct>();
            string commandText = "select * from collect";
            SQLiteCommand command = m_connect.CreateCommand();
            command.CommandText = commandText;
            var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                collectInfo = new CollectStruct();
                collectInfo.Href = reader["url"].ToString();
                collectInfo.Name = reader["name"].ToString();
                collectInfo.ImagePath = reader["imagePath"].ToString();
                list.Add(collectInfo);
            }
            return list;
        }

        public CollectStruct SearchCollectByName(string name,string url)
        {
            CollectStruct collectInfo;
            collectInfo = new CollectStruct();
            string sql = "select * from collect where name='" + name + "' and url='" + url+ "'";
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            var reader = command.ExecuteReader();
            collectInfo = new CollectStruct();

            while (reader.Read())
            {
                collectInfo.Href = reader["url"].ToString();
                collectInfo.Name = reader["name"].ToString();
                collectInfo.ImagePath = reader["imagePath"].ToString();
            }
            
            return collectInfo;
        }

        public bool DeleteCollect(CollectStruct info)
        {
            bool isSuccess = true;
            string sql = "delete from collect where name='" + info.Name + "' and url='" + info.Href + "'";
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            command.ExecuteNonQuery();
            return isSuccess;
        }

        public void CreateDirTemp(string tableName)
        {
            string sql = "create table " + tableName + " (name vchar(100), href text) ";
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            command.ExecuteNonQuery();  
        }

        public void InserDirectoryLink(string tableName, string dirName, string dirLink)
        {
            string sql = "insert into " + tableName + " (name, href) " + " values ";
            sql = sql + "(" + "'" + dirName + "'" + "," + "'" + dirLink + "')";
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            command.ExecuteNonQuery(); 
        }

        public Dictionary<string, string> GetDirectory(string tableName)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string sql = "select * from " + tableName;
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                if (dict.ContainsKey(reader["name"].ToString()) == false)
                {
                    dict.Add(reader["name"].ToString(), reader["href"].ToString());
                }
            }
            return dict;
        
        }

        public bool isHasTable(string tableName)//判断是否含有该表
        {
            bool isHas = false;
            string sql =  "SELECT COUNT(*) FROM sqlite_master where type='table' and name=" + "'" + tableName + "'";
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            int num = Convert.ToInt32(command.ExecuteScalar());
            
            if (num > 0)
            {
                isHas = true;
            }
            else
            {
                isHas = false;
            }
            return isHas;
        }

        public List<ResumeTask> CheckDownFinished(string tableName)
        {
            string sql = "";
            List<ResumeTask> taskList = new List<ResumeTask>();
            ResumeTask task = new ResumeTask();
            sql = "select taskName,url,isFinished,pageDownLoad from " + tableName;
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                if (reader["isFinished"].ToString() == "False")
                {
                    task = new ResumeTask();
                    task.ComicDic = reader["taskName"].ToString();
                    task.ComicUrl = reader["url"].ToString();
                    task.HasDownLoad = reader["pageDownLoad"].ToString();
                    taskList.Add(task);
                }
                
            }
            return taskList;
        }

        //更新某一字段数据
        public void UpdateData(string columnName, string columnNewValue, string columnOldValue)
        {
            string sql = "update task set " + columnName + " = "  + columnNewValue;
            sql = sql + " where " + columnName + " = " +  columnOldValue;
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            command.ExecuteNonQuery();
        }

        public void RemoveRow(string columnName)
        { 
            
        }

        public void CloseDataBase()
        {
            m_connect.Close();
        }
    }
}
