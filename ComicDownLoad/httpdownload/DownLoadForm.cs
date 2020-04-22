using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace comicDownLoad
{
    public partial class DownLoadForm : Form
    {
        DateTime time;
        NotifyIcon notify;
        List<DownListBox> downlistCollection = null;
        List<DownTask> taskQueue;

        public DownLoadForm()
        {
            InitializeComponent();
            downlistCollection = new List<DownListBox>();
            taskQueue = new List<DownTask>();
        }

        private void DownOnePicFinsihed(object sender, DownLoadArgs args)
        {
            double fileLen = args.FileSize / 1024;
            double useTime = DateTime.Now.Subtract(time).Milliseconds/1000;
            if (useTime <= 0)
                useTime = 1;
            double netSpeed = fileLen / useTime;
            UpdateDownLoadProgress(args.TaskName, args.DownLoadProgress);

            this.Invoke(new Action(() =>
            {
                this.Text = "瞬时速度:" + netSpeed.ToString() + "K/S";
                foreach (var i in downlistCollection)
                {
                    if (args.TaskName == i.Title)
                    {
                        i.ProgressVal++;
                    }
                }
            }));
            
        }

        private void ShowMessage(string caption)//展示通知信息
        {
            notify = new NotifyIcon();
            notify.Icon = new Icon("laba.ico");
            notify.Click += notify_Click;
            notify.Visible = true;
            notify.ShowBalloonTip(1000, "通知", caption, ToolTipIcon.Info);
        }

        void notify_Click(object sender, EventArgs e)
        {
            MessageBox.Show("你点击了");
        }

        private void DownAllFinished(object sender, DownLoadArgs args)
        {
            string caption = args.TaskName + " 下载完成！";
            SqlOperate operate = new SqlOperate();
            operate.CreateOrOpenDataBase("task.db");
            operate.UpdateData("isFinished", "1", args.TaskName);
            this.Invoke(new Action(() =>
            {
                this.Text = caption;
            }));
            
            operate.CloseDataBase();
            ShowMessage(caption);
        }

        private void AddDownRecord(string TaskName, string downLoadUrl, string savePath, int pageCount, int progress, int finished)
        {
            SqlOperate operate = new SqlOperate();
            operate.CreateOrOpenDataBase("task.db");
            operate.InsertData("task", TaskName, downLoadUrl, savePath, pageCount, progress, finished);
            operate.CloseDataBase();
        }

        private void DeleteDownRecord(string TaskName)
        {
            SqlOperate operate = new SqlOperate();
            operate.CreateOrOpenDataBase("task.db");
            operate.DeleteComicsTask(TaskName);
            operate.CloseDataBase();
        }

        private void UpdateDownLoadProgress(string taskName, int progress)
        {
            SqlOperate operate = new SqlOperate();
            operate.CreateOrOpenDataBase("task.db");
            operate.UpdateData("pageDownLoad", progress.ToString(), taskName);
            operate.CloseDataBase();
        }

        private void DownPause(object sender, DownLoadArgs args)
        {
            string caption = args.TaskName + "下载出错";
            ShowMessage(caption);
        }

        private List<ResumeTask> CheckIfAllDownFinished(List<DownLoadFile> downloadFile)//判断是否存在该任务
        {
            SqlOperate operate = new SqlOperate();
            operate.CreateOrOpenDataBase("task.db");
            var list = operate.CheckDownFinished("task");
            operate.CloseDataBase();
            return list;
        }

        private DownListBox SearchDownListBox(string name)
        {
            foreach (var i in downlistCollection)
            {
                if (i.Title == name)
                {
                    return i;
                }
            }

            return new DownListBox();
        }

        private void StartNewDownLoad(List<DownLoadFile> downloadFile, PublicThing decoder, int startIndex = 0)//开始全新下载
        {
            var fullPath = "";           
          
            try
            {                
                Task task = new Task(() =>
                {
                    DownListBox downItem = null;
                    List<DownListBox> downList;
                    downList = new List<DownListBox>();

                    foreach (var i in downloadFile)
                    {
                        DownTask downTask = new DownTask();
                        var url = i.ComicUrl;
                        var response = AnalyseTool.HttpGet(url);
                        var down = decoder.GetDownImageList(response);//解析图片真实地址

                        if (down.ImageList == null||down.Count == 0)
                        {
                            MessageBox.Show("该漫画已经下架", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        this.Invoke(new Action(() =>
                        {
                            filePanel.Controls.Add(downItem);
                            downItem = SearchDownListBox(i.ComicName);
                            downTask.SourceUrl = url;
                            downItem.deleteEvent += downItem_deleteEvent;
                            downItem.resumeDownLoadEvent += downItem_resumeDownLoadEvent;
                            downItem.SetMaxPage(down.ImageList.Count);//下载最大值
                            downItem.Title = i.ComicName;//漫画名字
                            downItem.Pages = down.ImageList.Count;
                        }));
                    

                        if (downlistCollection.Contains(downItem) == false)
                            downlistCollection.Add(downItem);//添加到控件集合

                        downTask.ComicName = i.ComicName;
                        downTask.downLoadOneFinished += DownOnePicFinsihed;//下载完成一个图片
                        downTask.downFinished += DownAllFinished;
                        downTask.downPaused += DownPause;
                        downTask.ImageFiles = down.ImageList;

                        if (File.Exists(i.SavePath) == false)
                        {
                            fullPath = i.SavePath + "\\" + i.ComicName + "\\";
                            Directory.CreateDirectory(i.SavePath + "\\" + i.ComicName + "\\");
                        }                    

                        downItem.FilePath = fullPath;
                        downTask.DownLoadPath = fullPath;
                        downTask.DownLoadStart(startIndex);                                       
                        taskQueue.Add(downTask);                    
                        AddDownRecord(downTask.ComicName, downTask.SourceUrl, downTask.DownLoadPath, downTask.ImageFiles.Count, 0, 0);//添加记录到数据库
                        downList.Add(downItem);
                    }

                    this.Invoke(new Action(() =>
                    {
                        filePanel.Controls.AddRange(downList.ToArray());
                    }));
                    
                });

                task.Start();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("开始下载出错:{0}", ex.Message);
            }
        }

        void downItem_resumeDownLoadEvent(object sender, EventArgs args)//继续下载
        {
            DownTask task = null;
            DownListBox listBox = sender as DownListBox;

            foreach (var i in taskQueue)
            {
                if (i.ComicName == listBox.Title)
                {
                    task = i;
                }
            }

            if (task == null)
            {
                SqlOperate operate = new SqlOperate();
                operate.CreateOrOpenDataBase("task.db");
                var list = operate.GetRecordList();

                foreach (var i in list)
                {
                    if (i.TaskName == listBox.Title)
                    {
                        List<DownLoadFile> downList = new List<DownLoadFile>();
                        DownLoadFile file = new DownLoadFile();
                        file.ComicName = i.TaskName;
                        file.ComicUrl = i.Url;
                        file.SavePath = i.Path;
                        downList.Add(file);
                        StartNewDownLoad(downList, DecoderDistrution.GiveDecoder(i.Url), i.DownLoadProgress - 1);
                    }
                }

                operate.CloseDataBase();
            }
        }

        void downItem_deleteEvent(object sender, EventArgs args)
        {
            DownListBox listBox = sender as DownListBox;
            StopDownTask(listBox.Title);
            DeleteComicDir(listBox.FilePath);
            DeleteDownRecord(listBox.Title);
            downlistCollection.Remove(listBox);
            filePanel.Controls.Remove(listBox);

        }

        void StopDownTask(string taskName)
        {
            DownTask task = null;

            foreach (var i in taskQueue)
            {
                if (i.ComicName == taskName)
                {
                    i.StopDownLoad();
                    task = i;
                }
            }

            if (task != null)
            {
                taskQueue.Remove(task);
            }
        }

        public static void DeleteDir(string file)
        {

            try
            {
                System.IO.DirectoryInfo fileInfo = new DirectoryInfo(file);
                fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;
                System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);

                if (Directory.Exists(file))
                {
                    foreach (string f in Directory.GetFileSystemEntries(file))
                    {
                        if (File.Exists(f))
                        {
                            File.Delete(f);
                            Console.WriteLine(f);
                        }
                        else
                        {
                            DeleteDir(f);
                        }

                    }

                    Directory.Delete(file);
                }

            }
            catch (Exception ex) // 异常处理
            {
                Console.WriteLine(ex.Message.ToString());// 异常信息
            }

        }

        void DeleteComicDir(string path)
        {
            if (Directory.Exists(path) == false)
                return;

            DeleteDir(path);          
        }

        private void ReLoad()
        {
            filePanel.Controls.Clear();
            LoadDownLoadRecord();
        }

        private void ResumeDownLoadTask(List<ResumeTask> resumeTask)//断点下载
        {
            foreach (var i in resumeTask)//
            { 
                
            }
        }

        private void StopDownLoad()//存在该任务，且已经下载完成
        { 
            
        }

        public void Start(List<DownLoadFile> downloadFile, PublicThing decoder)//批量下载任务没有实现
        {      
            SqlOperate sqlOperate = new SqlOperate();
            sqlOperate.CreateOrOpenDataBase("task.db");

            try
            {
                StartNewDownLoad(downloadFile, decoder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void newDownBtn_Click(object sender, EventArgs e)//新建下载
        {
            //newTask newTask = new newTask();
            //newTask.Show();
            DownListBox listBox = new DownListBox();
            listBox.deleteEvent += downItem_deleteEvent;
            filePanel.Controls.Add(listBox);
        }

        private void DownConfigBtn_Click(object sender, EventArgs e)
        {
          
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();

            if (folder.ShowDialog() == DialogResult.OK)
            {
                savePathTexb.Text = folder.SelectedPath;
                ComicConfig.SaveConfig("savePath", savePathTexb.Text);
            }
        }

        private void DownLoadForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void LoadDownLoadRecord()
        {
            DateTime time = DateTime.Now;
            SqlOperate operate = new SqlOperate();
            operate.CreateOrOpenDataBase("task.db");
            var list = operate.GetRecordList();
            Console.WriteLine("查找消耗时间:{0}", DateTime.Now.Subtract(time).Milliseconds);
            time = DateTime.Now;
            operate.CloseDataBase();

            List<DownListBox> itemList = new List<DownListBox>();

            foreach (var i in list)
            {
                DownListBox downBox = new DownListBox();
                downBox.deleteEvent += downItem_deleteEvent;
                downBox.resumeDownLoadEvent += downItem_resumeDownLoadEvent;
                downBox.SetMaxPage(i.PageCount);//下载最大值
                downBox.Title = i.TaskName;//漫画名字
                downBox.Pages = i.PageCount;
                downBox.CurrentPage = i.DownLoadProgress;
                downBox.FilePath = i.Path;
                itemList.Add(downBox);
                downlistCollection.Add(downBox);
            }

            filePanel.Controls.AddRange(itemList.ToArray());        
            Console.WriteLine("绘制消耗时间:{0}", DateTime.Now.Subtract(time).Milliseconds);
           
        }

        private void DownLoadForm_Load(object sender, EventArgs e)
        {
            if (File.Exists("CONFIG.INI"))
            {
                 savePathTexb.Text = ComicConfig.ReadConfig("savePath");
            }

            LoadDownLoadRecord();
        }

        private void proxyCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (proxyCheck.Checked)
            {             
                ipPortTextBox.Enabled = true;
                portTextBox.Enabled = true;
            }
            else
            {
                ipPortTextBox.Enabled = false;
                portTextBox.Enabled = false;
            }
        }

    }


}
