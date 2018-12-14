using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Web;
using System.Threading;

namespace comicDownLoad
{
    public partial class MainForm : Form
    {
        Thread getThread;
        SqlOperate opt;
        DownLoadForm downForm;
        Resourse resourse;
        ImageList showImageList;
        PublicThing decoder;
        string downLoadPath = "";
        bool m_exitLoad = true;
        Dictionary<string, string> downUrlDic;

        delegate void downloadDlegate(Queue<string> url, string comicName);
        delegate void ProcessbarDelegate(ProgressBar progressBar);
        delegate void AddListViewItem(ListViewItem item, Image image, ListView addView);
        delegate void AddPanel(CheckBox check);
        private static object objLock = new object();

        public MainForm()
        {
            InitializeComponent();           
        }

        public void StartDownLoad()
        { 
            
        }

        public void GetImagesList(object b)//获取批量图片
        {
            var count = 0;
            var comicDic = new Dictionary<string, string>();
            Image image = null;
            ViewStruct view = (ViewStruct)b;
            Queue<BasicComicInfo> queue = view.Queue;          
            ProcessbarDelegate progress = new ProcessbarDelegate(AddProgress);
            AddListViewItem addItem = new AddListViewItem(ListViewItemAdd);
            ListViewItem item = null;

            lock (objLock)
            {
                this.Invoke(new Action(() =>
                {
                    m_exitLoad = false;
                    resultListView.Items.Clear();
                    showImageList.Images.Clear();
                   view.ViewAdd.LargeImageList = showImageList;
                }));

                foreach (var i in queue)
                {
                    if (m_exitLoad == false)
                    {
                        progressBar1.Invoke(progress, progressBar1);
                        image = AnalyseTool.GetImage(i.ComicImgUrl);
                        item = new ListViewItem();
                        item.Text = i.ComicName;
                        item.ImageIndex = count++;
                        // Console.WriteLine("漫画名字:{0},图片索引:{1}", item.Text, item.ImageIndex);
                        this.Invoke(addItem, item, image, view.ViewAdd);

                        if (comicDic.ContainsKey(i.ComicName) == false)
                            comicDic.Add(i.ComicName, i.ComicHref);

                        resourse.SearchResultURL = comicDic;
                    }
                    else
                    {
                        count = 0;
                        m_exitLoad = true;
                        return;
                    }                 
                }

                m_exitLoad = true;
                runGif.Invoke(new Action(() =>
                {
                    runGif.Visible = false;
                }));
                
            }
            
        }

        private void ListViewItemAdd(ListViewItem item, Image image, ListView addView)
        {
            if (image != null)
            {
                showImageList.Images.Add(image);
                addView.Items.Add(item);
            }
        }
    
        private void DecodeCategory(string listUrl)//解析每一栏的标签页
        {
            var msg = AnalyseTool.HttpGet(listUrl);
            decoder = DecoderDistrution.GiveDecoder(listUrl);
            var cateCollect = decoder.FindComicByCategory(msg);
            resourse.NextPage = cateCollect.NextPageUrl;
            progressBar1.Value = 0;
            progressBar1.Maximum = cateCollect.ComicQueue.Count;
            resultListView.Items.Clear();
            ViewStruct view = new ViewStruct();
            view.Queue = cateCollect.ComicQueue;
            view.ViewAdd = resultListView;

            if (m_exitLoad == false)
                m_exitLoad = true;

            getThread = new Thread(new ParameterizedThreadStart(GetImagesList));
            getThread.Start(view);      
        }
      
        private void AddProgress(ProgressBar pro1)
        {
            if (pro1.Value < pro1.Maximum)
                pro1.Value++;
            else
                pro1.Value = 0;

            statusText.Text = pro1.Value.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            resourse = new Resourse();
            showImageList = new ImageList();
            showImageList.ImageSize = new Size(137, 174);
            showImageList.ColorDepth = ColorDepth.Depth32Bit;
            opt = new SqlOperate();
        }

        private void DownComic(object sender, EventArgs e)
        {
            if (downForm == null)
            {
                downForm = new DownLoadForm();
                downForm.StartPosition = FormStartPosition.Manual;
                downForm.Location = new Point(this.Left + 55, this.Bottom - downForm.Height - 50);
                downForm.Show();
            }
            else
            {
                downForm.Show();
            }
        }

        private void nextBtn_Click(object sender, EventArgs e)
        {
            if (resourse.NextPage != null)
            {
                DecodeCategory(resourse.NextPage);
            }
        }

        private void lastBtn_Click(object sender, EventArgs e)
        {
            mainFrame.SelectedPageIndex = 0;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            #region 双击调用浏览器打开网页
            mainFrame.SelectedPageIndex = 0;
            /*if (resourse.SearchResultURL.ContainsKey(resultListView.SelectedItems[0].Text))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = @"D:\Program Files (x86)\360Chrome\Chrome\Application\360chrome.exe";
                //startInfo.FileName = @"D:\临时\漫画\ComicsViewer_chn\ComicsViewer.exe";
                startInfo.Arguments = resourse.SearchResultURL[resultListView.SelectedItems[0].Text];
                //startInfo.Arguments = @"D:\我们是变态 001集\0.jpg";
                Process.Start(startInfo);
            }*/
            #endregion
        }

        private void button3_Click(object sender, EventArgs e)
        {
          
        }
      
        private void SetProgressBar(Queue<BasicComicInfo> queue)//设置进度条
        {
            this.Invoke(new Action(() =>
            {
                progressBar1.Value = 0;
                progressBar1.Maximum = queue.Count;
                resultListView.Items.Clear();
                ViewStruct view = new ViewStruct();
                view.Queue = queue;
                view.ViewAdd = resultListView;

                if(m_exitLoad == false)
                    m_exitLoad = true;

                getThread = new Thread(new ParameterizedThreadStart(GetImagesList));
                getThread.Start(view);
            }));
                      
        }

        private void HotComic(string url, PublicThing comicDecoder)
        {
            mainFrame.SelectedPageIndex = 0;
            tabControl1.TabPages.Clear();
            runGif.Visible = true;

            Task task = new Task(() =>
            {
                var response = AnalyseTool.HttpGet(url);
                
                if (response == "")
                {
                    runGif.Invoke(new Action(() =>
                    {
                        runGif.Visible = false;
                    }));
                    return;
                }
                var comicTop = comicDecoder.GetTopComic(response);
                SetProgressBar(comicTop);
            });

            task.Start();
           
        }

        private void FenLei(string url, PublicThing comicDecoder)//通用分类方法
        {
            var count = 0;
            DevExpress.XtraTab.XtraTabPage tab;
            mainFrame.SelectedPageIndex = 0;

            Task task = new Task(() =>
            {
                var response = AnalyseTool.HttpGet(url);

                if (response == "")
                {
                    MessageBox.Show("加载图片失败");
                    return;
                }
                
                tabControl1.Invoke(new Action(() =>
                {
                    var cateInfo = comicDecoder.GiveCategoryInfo(response);
                    tabControl1.TabPages.Clear();
                    resourse.CategoryCollect = cateInfo.ComicList;

                    foreach (var i in cateInfo.ComicList)
                    {
                        tab = new DevExpress.XtraTab.XtraTabPage();
                        tab.Text = i.Key;
                        tabControl1.TabPages.Add(tab);
                        if (count++ > 15)
                            break;
                    }
                }));
            
                if (resourse.CategoryCollect.Count > 0)
                {
                    var key = "";

                    foreach (var i in resourse.CategoryCollect)
                    {
                        key = i.Key;
                        break;
                    }
                }
            });

            task.Start();                 
        }

        private void comicNavBar_LinkClicked(object sender, DevExpress.XtraNavBar.NavBarLinkEventArgs e)
        {
            string url = "";  

            if (e.Link.Caption == "分类")
            {
                switch (e.Link.Group.Name)
                {
                    case "jingmingyan": url = "http://www.jmydm.com/ManhuaClass-LangmanAiqing/"; break;
                    case "kanmanhua": url = "https://www.manhuagui.com/list/wanjie/"; break;
                    case "hanhanmanhua": url = "http://www.hhimm.com/comic/class_1.html"; break;
                    case "dongman": url = "http://www.dmzx.com/rexue-all-all-all-all/"; break;
                    case "yylsmanhua": url = "http://8comic.se/category/%E6%BC%AB%E7%95%AB%E5%88%86%E9%A1%9E/%E5%86%92%E9%9A%AA/"; break;
                    case "k886": url = "https://www.k886.net/index-html-status-0-typeid-0-sort-"; break;
                    case "feichangaiman": url = "http://www.verydm.com/index.php?r=comic/list&show=grid&sort=update"; break;
                    case "manhuaren": url = "http://www.1kkk.com/manhua-list/"; break;
                }

                if (url.Length > 0)
                {
                    decoder = DecoderDistrution.GiveDecoder(url);
                    FenLei(url, decoder);
                }
            }
            else
            {
                if (e.Link.Caption == "热门")
                {
                    switch (e.Link.Group.Name)
                    {
                        case "jingmingyan": url = "http://www.jmydm.com"; break;
                        case "kanmanhua": url = "https://www.manhuagui.com"; break;
                        case "hanhanmanhua": url = "http://www.hhimm.com/"; break;
                        case "yylsmanhua": url = "http://8comic.se/"; break;
                        case "dongman": url = "http://www.dmzx.com/"; break;
                        case "k886": url = "https://www.k886.net/"; break;
                        case "feichangaiman": url = "http://www.verydm.com/"; break;
                        case "manhuaren": url = "http://www.1kkk.com/manhua-jp/"; break;
                    }

                    if (url.Length > 0)
                    {
                        decoder = DecoderDistrution.GiveDecoder(url);
                        HotComic(url, decoder);
                    }
                }

                if (e.Link.Caption == "推荐")
                {
                    url = "http://www.hhmmoo.com/comic/best_1.html";
                    decoder = DecoderDistrution.GiveDecoder(url);
                    HotComic(url, decoder);
                }
            }
          
        }

        private void tabControl1_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (resourse.CategoryCollect != null && tabControl1.SelectedTabPage != null)
            {
                DecodeCategory(resourse.CategoryCollect[tabControl1.SelectedTabPage.Text]);
            }
        }

        private void FillComicInfo(ListView selectView)
        {         
            int x = 0;
            int y = 0;
            var receive = "";
            mainFrame.SelectedPageIndex = 1;
            comicNameLabel.Text = "漫画名：" + selectView.SelectedItems[0].Text;
            var url = resourse.SearchResultURL[selectView.SelectedItems[0].Text];
            resourse.ComicName = selectView.SelectedItems[0].Text;
            resourse.ComicHref = url;
            decoder = DecoderDistrution.GiveDecoder(url);
            receive = AnalyseTool.HttpGet(url);
            
            if (receive == "")
            {
                MessageBox.Show("网络错误，获取失败");
                return;
            }
            var comicInfo = decoder.GetComicInfo(receive);
            downUrlDic = new Dictionary<string, string>();//存储当前漫画所有图片链接
            downUrlDic = comicInfo.URLDictionary;
            comicPicBox.Image = showImageList.Images[selectView.SelectedItems[0].Index];
            authorLab.Text = (comicInfo.Author == null || comicInfo.Author.Length == 0)? "略":comicInfo.Author;
            tagLabe.Text = "标签：  " + comicInfo.Tag;
            descLable.Text = "简介：" + comicInfo.Description;
            statusLab.Text = "连载状态：" + comicInfo.HasFinished;
            CheckBox check;
            checkPanel.Controls.Clear();

            Task task = new Task(() =>
            {

                foreach (var i in comicInfo.URLDictionary)
                {
                    check = new CheckBox();
                    check.AutoSize = true;
                    check.Text = i.Key;
                    check.Location = new Point(x, y);

                    this.Invoke(new Action(() =>
                    {
                        checkPanel.Controls.Add(check);
                    }));


                    if (x + check.Width + 5 > checkPanel.Width - check.Width - 10)//如果
                    {
                        x = 0;
                        y = y + check.Height + 20;
                    }
                    else
                    {
                        x = x + check.Width + 5;
                    }
                }

            });

            task.Start();
        }

        private void resultListView_DoubleClick(object sender, EventArgs e)//需要优化
        {
            if (resultListView.SelectedItems[0].Text != null)
            {              
                FillComicInfo(resultListView);
            }
        }

        public static string BitStringDeal(string data)
        {
            var ret = "";
            var temp = "";
            var str = new StringBuilder();
            string source = Convert.ToString(Convert.ToInt32(data, 16), 2).PadLeft(16, '0');

            for (int i = source.Length-4; i >= 0; i = i - 4)
            {
                Console.WriteLine("i:{0}",i);
                ret = source.Substring(i, 4);
                str.Append(Convert.ToInt32(ret,2).ToString("X"));
            }
            return str.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ComicReader comic = new ComicReader();
            comic.Show();
            //BitStringDeal("10EE");
            /*KanmanhuaK886 kan = new KanmanhuaK886();
            var ret = AnalyseTool.HttpGet("https://www.k886.net/index-look-cid-28471-id-224254");                     
            kan.GetDownImageList(ret);*/
        }

        private void downnLoadTool_Click(object sender, EventArgs e)//下载漫画
        {
            try
            {
                DownLoadFile downFile;
                List<DownLoadFile> downFileList;
                
                downFileList = new List<DownLoadFile>();

                if (downForm == null)
                {
                    downForm = new DownLoadForm();
                }

                foreach (CheckBox i in checkPanel.Controls)
                {
                    if (i.Checked)
                    {
                        if (downUrlDic.ContainsKey(i.Text))
                        {
                            downFile = new DownLoadFile();
                            downFile.ComicUrl = downUrlDic[i.Text];                           
                            downFile.ComicName = comicNameLabel.Text.Substring(4) + i.Text;
                            downFile.SavePath = "D:\\漫画\\";
                            downFileList.Add(downFile);
                        }
                    }
                }

                downForm.StartPosition = FormStartPosition.Manual;
                downForm.Location = new Point(this.Left + 55, this.Bottom - downForm.Height - 50);
                downForm.Start(downFileList, decoder);
                downForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void fullChoiceTool_Click(object sender, EventArgs e)
        {
            foreach (CheckBox i in checkPanel.Controls)
            {
                i.Checked = true;
            }
        }

        private void unChoiceTool_Click(object sender, EventArgs e)
        {
            foreach (CheckBox i in checkPanel.Controls)
            {
                i.Checked = false;
            }
        }

        private void SearchComic(string response)
        {
            var result = decoder.GetSearchComic(response);           

            this.Invoke(new Action(() =>
            {
                progressBar1.Value = 0;
                progressBar1.Maximum = result.SearchQueue.Count;
                resultLabel.Text = "共找到：" + result.Count.ToString() + "本漫画";
                searchListView.Items.Clear();
                mainFrame.SelectedPageIndex = 2;

            }));
            
            ViewStruct view = new ViewStruct();
            view.Queue = result.SearchQueue;
            view.ViewAdd = searchListView;
            getThread = new Thread(new ParameterizedThreadStart(GetImagesList));
            getThread.Start(view);
        }

        private void searchBtn_Click(object sender, EventArgs e)
        {
            var response = "";
            var url = "";
            var name = "";
            var keyWord = HttpUtility.UrlEncode(searchControl.Text, Encoding.UTF8);
            RadioButton[] radio = {k886Check,kanCheck,hanhanCheck,jingmingyanCheck};
            
            foreach (RadioButton r in radio)
            {
                if (r.Checked)
                {
                    name = r.Name;
                    break;
                }
            }

            Task task = new Task(() =>
            {
                switch(name)
                {
                    case "hanhanCheck": url = "http://www.hhmmoo.com/comic/?act=search&st=" + keyWord;break;
                    //case "jingmingyanCheck": url = "http://www.jmydm.com/comic/s.aspx"; response = AnalyseTool.PostWebRequest(url, "tbSTxt=" + keyWord);break;
                    case "kanCheck": url = "http://www.manhuagui.com/s/" + keyWord + ".html"; break;
                    case "k886Check": url = "https://www.k886.net/search-index?searchType=1&q=" + keyWord; break;
                }

                response = AnalyseTool.HttpGet(url);
                decoder = DecoderDistrution.GiveDecoder(url);
                SearchComic(response);

            });
            task.Start();           
        }

        private void searchListView_DoubleClick(object sender, EventArgs e)
        {
            if (searchListView.SelectedItems[0].Text != null)
            {
                FillComicInfo(searchListView);
            }
        }

        private void homeBtn_Click(object sender, EventArgs e)
        {
            mainFrame.SelectedPageIndex = 0;
        }

        private void lastPageBtn_Click(object sender, EventArgs e)
        {
            if (resourse.NextPage != null && resourse.NextPage != "")
            {
                DecodeCategory(resourse.NextPage);
            }
            else
            {
                MessageBox.Show("没有下一页");
            }
        }

        private void readerBtn_Click(object sender, EventArgs e)
        {
            ComicReader comic = new ComicReader();
            comic.Show();
        }

        private void startReadBtn_Click(object sender, EventArgs e)
        {

            DownLoadFile downFile = null ;
            DownTask downTask;
            ComicReader reader = new ComicReader();

            foreach (CheckBox i in checkPanel.Controls)
            {
                if (i.Checked)
                {
                    if (downUrlDic.ContainsKey(i.Text))
                    {
                        downFile = new DownLoadFile();
                        downFile.ComicUrl = downUrlDic[i.Text];
                        downFile.ComicName = comicNameLabel.Text.Substring(4) + i.Text;
                        downFile.SavePath = "temp\\";
                        break;
                    }
                }
            }

            if (downFile == null)
                return;

            downTask = new DownTask();
            downTask.addFileEvent += reader.AddFileToList;
            downTask.ComicName = downFile.ComicName;
            downTask.DownLoadPath = downFile.SavePath;
            downTask.ReferUrl = downFile.ComicUrl;
            downTask.GetNetComic(downFile);
            reader.StartRead();
            reader.Show();
        }

        private void addFavrourate_Click(object sender, EventArgs e)
        {
            SqlOperate opt = new SqlOperate();
            SqlOperate.CollectStruct collect;
            collect = new SqlOperate.CollectStruct();
            collect.Href = resourse.ComicHref;
            collect.Name = resourse.ComicName;
            collect.ImagePath = Directory.GetCurrentDirectory() + "\\收藏\\" + collect.Name + ".jpg";
            

            if (comicPicBox.Image != null)
            {
                if (Directory.Exists("收藏") == false)
                {
                    Directory.CreateDirectory("收藏");
                }

                comicPicBox.Image.Save(collect.ImagePath);
            }
                        
            if (opt.AddCollect(new List<SqlOperate.CollectStruct>{collect}))
            {
                MessageBox.Show("收藏漫画成功!");
            }
        }

        private void collectBtn_Click(object sender, EventArgs e)
        {
            var count = 0;
            Image image;
            SqlOperate opt = new SqlOperate();
            FileStream stream;
            ListViewItem item = null;
            var list = opt.GetCollet();
            showImageList.Images.Clear();
            resultListView.Items.Clear();
            mainFrame.SelectedPageIndex = 0;
            m_exitLoad = true;

            if (list.Count > 0)
            {
                resourse.SearchResultURL = new Dictionary<string, string>();
            }

            foreach(var i in list)
            {               
                item = new ListViewItem();
                item.Text = i.Name;
                item.ImageIndex = count++;
                stream = new FileStream(i.ImagePath,FileMode.Open, FileAccess.Read);
                image = Image.FromStream(stream);
                showImageList.Images.Add(image);
                resultListView.Items.Add(item);
                stream.Close();

                if (resourse.SearchResultURL.ContainsKey(i.Name) == false)
                    resourse.SearchResultURL.Add(i.Name,i.Href);
            }

            resultListView.LargeImageList = showImageList;
        }

        private void reamoveCollect_Click(object sender, EventArgs e)
        {
            SqlOperate.CollectStruct collectInfo;

            foreach (ListViewItem i in resultListView.SelectedItems)
            {
                resultListView.Items.Remove(i);
                collectInfo = opt.SearchCollectByName(i.Text, resourse.SearchResultURL[i.Text]);
                resourse.SearchResultURL.Remove(i.Text);
                showImageList.Images[i.ImageIndex].Dispose();
                showImageList.Images.RemoveAt(i.ImageIndex);
                File.Delete(collectInfo.ImagePath);
                opt.DeleteCollect(collectInfo);
            }
        }

        private void LastPage()
        {
            if (mainFrame.SelectedPageIndex - 1 >= 0)
                mainFrame.SelectedPageIndex = mainFrame.SelectedPageIndex - 1;
        }

        private void skinButton1_Click(object sender, EventArgs e)
        {
            LastPage();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Back: LastPage(); break;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

    }

    public class ViewStruct
    {
        public Queue<BasicComicInfo> Queue { get; set; }
        public ListView ViewAdd { get; set; }
    }
}
