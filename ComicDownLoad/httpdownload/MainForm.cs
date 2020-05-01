using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Web;
using Imazen.WebP;
using System.Threading;

namespace comicDownLoad
{
    public partial class MainForm : Form
    {
        Thread getThread;
        SqlOperate opt;
        DownLoadForm downForm;
        Resource resource;
        ImageList showImageList;//图片集合
        PublicThing decoder;
        bool m_exitLoad = true;//是否退出标识
        Dictionary<string, string> downUrlDic;

        delegate void downloadDlegate(Queue<string> url, string comicName);
        delegate void ProcessbarDelegate(ProgressBar progressBar);
        delegate void AddListViewItem(ListViewItem item, Image image, ListView addView);
        delegate void AddPanel(CheckBox check);
        private static object objLock = new object();
        private object getLock = new object();

        public MainForm()
        {
            InitializeComponent();           
        }

        private void SetGifHidden()
        {
            runGif.Invoke(new Action(() =>
            {
                runGif.Visible = false;
                progressBar1.Visible = false;
            }));
        }

        private void SetGifVisable()
        {
            runGif.Invoke(new Action(() =>
            {
                runGif.Visible = true;
                progressBar1.Visible = true;
            }));
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

                DateTime time = DateTime.Now;

                foreach (var i in queue)
                {
                    if (m_exitLoad == false)
                    {
                        progressBar1.Invoke(progress, progressBar1);
                        image = AnalyseTool.GetImage(i.ComicImgUrl);
                        item = new ListViewItem();
                        item.Text = i.ComicName;
                        item.ImageIndex = count++;
                        this.Invoke(addItem, item, image, view.ViewAdd);

                        if (comicDic.ContainsKey(i.ComicName) == false)
                            comicDic.Add(i.ComicName, i.ComicHref);

                        resource.SearchResultURL = comicDic;
                    }
                    else
                    {
                        count = 0;
                        m_exitLoad = true;
                        SetGifHidden();
                        return;
                    }                 
                }

                //Console.WriteLine("轮询图像获取:{0} ms", DateTime.Now.Subtract(time).Milliseconds);
                m_exitLoad = true;
                SetGifHidden();

            }
         
        }

        private void ListViewItemAdd(ListViewItem item, Image image, ListView addView)
        {
            if (image != null && !m_exitLoad)
            {
                showImageList.Images.Add(image);
                addView.Items.Add(item);
            }
        }
    
        private void DecodeCategory(string listUrl)//解析每一栏的标签页
        {
            Task task = new Task(() =>
            {
                var msg = AnalyseTool.HttpGet(listUrl);
                decoder = DecoderDistrution.GiveDecoder(listUrl);
                decoder.BackUrl = listUrl;
                var cateCollect = decoder.FindComicByCategory(msg);
                resource.NextPage = cateCollect.NextPageUrl;
                resource.LastPage = cateCollect.LastPageUrl;

                progressBar1.Invoke(new Action(() =>
                {
                    progressBar1.Value = 0;
                    progressBar1.Maximum = cateCollect.ComicQueue.Count;
                    resultListView.Items.Clear();
                }));
                
                ViewStruct view = new ViewStruct();
                view.Queue = cateCollect.ComicQueue;
                view.ViewAdd = resultListView;

                if (m_exitLoad == false)
                    m_exitLoad = true;

                getThread = new Thread(new ParameterizedThreadStart(GetImagesList));
                getThread.Start(view);
            });
            task.Start();

            
        }
      
        private void AddProgress(ProgressBar pro1)
        {
            if (pro1.Value < pro1.Maximum)
                pro1.Value++;
            else
                pro1.Value = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            resource = new Resource();
            showImageList = new ImageList();
            showImageList.ImageSize = new Size(137, 174);
            showImageList.ColorDepth = ColorDepth.Depth32Bit;
            opt = new SqlOperate();
            string url = "http://m.90mh.com/update/";
            HotComic(url, DecoderDistrution.GiveDecoder(url));
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
            if (resource.NextPage != null)
            {
                DecodeCategory(resource.NextPage);
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
            #endregion
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
            SetGifVisable();

            Task task = new Task(() =>
            {
                try
                {
                    lock (getLock)
                    {
                        var response = AnalyseTool.HttpGet(url);

                        if (response == "")
                        {
                            SetGifHidden();
                            return;
                        }
                        var comicTop = comicDecoder.GetTopComic(response);
                        SetProgressBar(comicTop);
                    }
                }
                catch(Exception ex)
                {

                }
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
                try
                {
                    lock (getLock)
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
                            resource.CategoryCollect = cateInfo.ComicList;

                            foreach (var i in cateInfo.ComicList)
                            {
                                tab = new DevExpress.XtraTab.XtraTabPage();
                                tab.Text = i.Key;
                                tabControl1.TabPages.Add(tab);

                                if (count++ > 15)
                                    break;
                            }
                        }));

                        if (resource.CategoryCollect.Count > 0)
                        {
                            var key = "";

                            foreach (var i in resource.CategoryCollect)
                            {
                                key = i.Key;
                                break;
                            }
                        }
                    }
                }
                catch
                {

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
                    case "feichangaiman": url = "http://comic.veryim.com/all/"; break;
                    case "manhuaren": url = "http://www.1kkk.com/manhua-list/"; break;
                    case "jiu0ManHua": url = "http://m.90mh.com/search/"; break;
                    case "veryDm": url = "http://www.veryimapp.com/index.php?r=comic/list&show=grid&sort=update"; break;
                    case "huhumanhua": url = "http://www.huhudm.com/comic/"; break;
                    case "manhuadui": url = "https://www.manhuadui.com/list/riben/"; break;
                    case "mangabzName": url = "http://www.mangabz.com/manga-list/";break;
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
                        //case "jingmingyan": url = "http://www.jmydm.com"; break;
                        case "kanmanhua": url = "https://www.manhuagui.com"; break;
                        case "hanhanmanhua": url = "http://www.hhimm.com/"; break;
                        case "yylsmanhua": url = "http://8comic.se/"; break;
                        case "dongman": url = "http://www.dmzx.com/"; break;
                        case "k886": url = "https://www.k886.net/"; break;
                        case "feichangaiman": url = "http://www.verydm.com/"; break;
                        case "manhuaren": url = "http://www.1kkk.com/manhua-jp/"; break;
                        case "manhuadui": url = "https://www.manhuadui.com/list/riben/"; break;
                        case "huhumanhua": url = "http://www.huhudm.com/"; break;
                        case "veryDm": url = "http://www.veryimapp.com/"; break;
                        case "jiu0ManHua": url = "http://m.90mh.com/update/"; break;
                        case "mangabzName": url = "http://www.mangabz.com"; break;
                        //case "jiu0ManHua": url = "http://m.90mh.com/update/"; break;
                    }

                    if (url.Length > 0)
                    {
                        decoder = DecoderDistrution.GiveDecoder(url);
                        HotComic(url, decoder);
                    }
                }
            }
          
        }

        private void tabControl1_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (resource.CategoryCollect != null && tabControl1.SelectedTabPage != null)
            {
                DecodeCategory(resource.CategoryCollect[tabControl1.SelectedTabPage.Text]);
            }
        }

        private void FillComicInfo(ListView selectView)
        {         
            var receive = "";
            var url = "";

            if(selectView.SelectedItems == null)
            {
                return;
            }

            this.Invoke(new Action(() =>
            {
                mainFrame.SelectedPageIndex = 1;
                comicNameLabel.Text = "漫画名：" + selectView.SelectedItems[0].Text;
                url = resource.SearchResultURL[selectView.SelectedItems[0].Text];
                resource.ComicName = selectView.SelectedItems[0].Text;
                resource.ComicHref = url;
            }));
                
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
           
            this.Invoke(new Action(() =>
            {
                CheckBox check;
                comicPicBox.Image = showImageList.Images[selectView.SelectedItems[0].Index];
                authorLab.Text = (comicInfo.Author == null || comicInfo.Author.Length == 0) ? "略" : comicInfo.Author;
                tagLabe.Text = "标签：  " + comicInfo.Tag;
                descLable.Text = "简介：" + comicInfo.Description;
                statusLab.Text = "连载状态：" + comicInfo.HasFinished;          
                checkPanel.Controls.Clear();

                if(comicInfo.URLDictionary == null)
                {
                    descLable.Text = "简介：" + "该漫画已经下架！";
                    return;
                }

                List<CheckBox> list = new List<CheckBox>();

                foreach (var i in comicInfo.URLDictionary)
                {
                    check = new CheckBox();
                    check.AutoSize = true;
                    check.Text = i.Key;
                    list.Add(check);
                    
                }
                checkPanel.Controls.AddRange(list.ToArray());
                SetGifHidden();                
            }));
                
        }

        private void resultListView_DoubleClick(object sender, EventArgs e)//需要优化
        {
            m_exitLoad = true;
            SetGifVisable();
            progressBar1.Value = 0;

            if (resultListView.SelectedItems[0].Text != null)
            {
                Task task = new Task(() =>
                {
                    FillComicInfo(resultListView);
                });
                task.Start();
                
            }
        }

        private void downnLoadTool_Click(object sender, EventArgs e)//下载漫画
        {
            try
            {
                string savaPath = "";
                DownLoadFile downFile;
                List<DownLoadFile> downFileList;
                
                downFileList = new List<DownLoadFile>();

                if (downForm == null)
                {
                    downForm = new DownLoadForm();
                }

                if (File.Exists("CONFIG.INI"))
                {
                    savaPath = ComicConfig.ReadConfig("savePath");
                }
                else
                {
                    savaPath = ".\\";
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
                            downFile.SavePath = savaPath;
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
            //resource.NextPage = result.NextPageUrl;
            //resource.LastPage = result.LastPageUrl;
            getThread = new Thread(new ParameterizedThreadStart(GetImagesList));
            getThread.Start(view);
        }

        private void searchBtn_Click(object sender, EventArgs e)
        {
            var response = "";
            var url = "";
            var name = "";
            var keyWord = HttpUtility.UrlEncode(searchControl.Text, Encoding.UTF8);
            RadioButton[] radio = { manhuaduiCheck, hanhanCheck, jiulingCheck};
            
            foreach (RadioButton r in radio)
            {
                if (r.Checked)
                {
                    name = r.Name;
                    break;
                }
            }

            SetGifVisable();

            Task task = new Task(() =>
            {
                lock (getLock)
                {
                    switch (name)
                    {
                        case "hanhanCheck": url = "http://www.hhimm.com/comic/?act=search&st=" + keyWord; break;
                        case "manhuarenCheck": url = "http://www.1kkk.com/search?title=" + keyWord; break;
                        case "k886Check": url = "https://www.k886.net/search-index?searchType=1&q=" + keyWord; break;
                        case "jiulingCheck": url = "http://m.90mh.com/search/?keywords=" + keyWord; break;
                        case "veryDmCheck": url = "http://www.veryimapp.com/index.php?r=comic%2Fsearch&keyword=" + keyWord; break;
                        case "manhuaduiCheck": url = "https://www.manhuadui.com/search/?keywords=" + keyWord; break;
                    }

                    response = AnalyseTool.HttpGet(url);
                    decoder = DecoderDistrution.GiveDecoder(url);
                    SearchComic(response);
                }
                
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
            if (resource.NextPage != null && resource.NextPage != "")
            {
                DecodeCategory(resource.NextPage);
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
            downTask.SourceUrl = downFile.ComicUrl;
            downTask.GetNetComic(downFile);
            reader.StartRead();
            reader.Show();
        }

        private void addFavrourate_Click(object sender, EventArgs e)
        {
            SqlOperate opt = new SqlOperate();
            SqlOperate.CollectStruct collect;
            collect = new SqlOperate.CollectStruct();
            collect.Href = resource.ComicHref;
            collect.Name = resource.ComicName;
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
                resource.SearchResultURL = new Dictionary<string, string>();
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

                if (resource.SearchResultURL.ContainsKey(i.Name) == false)
                    resource.SearchResultURL.Add(i.Name,i.Href);
            }

            resultListView.LargeImageList = showImageList;
        }

        private void reamoveCollect_Click(object sender, EventArgs e)
        {
            try
            {
                SqlOperate.CollectStruct collectInfo;

                foreach (ListViewItem i in resultListView.SelectedItems)
                {
                    resultListView.Items.Remove(i);
                    collectInfo = opt.SearchCollectByName(i.Text, resource.SearchResultURL[i.Text]);
                    resource.SearchResultURL.Remove(i.Text);

                    if (showImageList.Images.Count >= i.ImageIndex)
                    {
                        showImageList.Images[i.ImageIndex].Dispose();
                        showImageList.Images.RemoveAt(i.ImageIndex);
                    }

                    if (collectInfo.ImagePath != null)
                    {
                        File.Delete(collectInfo.ImagePath);
                        opt.DeleteCollect(collectInfo);
                    }
                }
            }
            catch
            {

            }
        }

        private void LastPage()
        {
            if (resource.LastPage != null)
            {
                DecodeCategory(resource.LastPage);
            }
            else
            {
                if (mainFrame.SelectedPageIndex - 1 >= 0)
                    mainFrame.SelectedPageIndex = mainFrame.SelectedPageIndex - 1;
            }
            
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

        private void ShowWait()
        {
            LodingWidow lodingWidow = new LodingWidow();
            lodingWidow.Location = new Point(this.Width/2, this.Height/2);
            lodingWidow.TopLevel = false;
            lodingWidow.Show();
            //lodingWidow.ShowDialog();
            this.Controls.Add(lodingWidow);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //Task task = new Task(() =>
            //{
            //    while(true)
            //    {
            //        string url = "http://www.mangabz.com";
            //        decoder = DecoderDistrution.GiveDecoder(url);
            //        HotComic(url, decoder);
            //        Thread.Sleep(1000 * 20);
            //    }

            //});
            //task.Start();

            //string url = "http://www.mangabz.com/m10344/";
            //url = "http://www.mangabz.com/m115462/";
            //var response = AnalyseTool.HttpGet(url);
            //PublicThing decoder = DecoderDistrution.GiveDecoder(url);      
            //decoder.GetDownImageList(response);
            ShowWait();
        }

        private void manhuaduiCheck_CheckedChanged(object sender, EventArgs e)
        {

        }
    }

    public class ViewStruct
    {
        public Queue<BasicComicInfo> Queue { get; set; }
        public ListView ViewAdd { get; set; }
    }
}
