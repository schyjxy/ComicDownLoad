using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using CCWin.SkinControl;
using System.Threading;
using System.Runtime.InteropServices;

namespace comicDownLoad
{
    public partial class ComicReader : Form
    {    
        private int sum;
        private int m_Page;
        private int m_clientWidth;
        private int m_clientHeight;
        private int m_posY;//滚动Y轴记录位置
        private bool isAdd;
        private double time = 0;
        private double scale = 1.0;
        private Image m_showImage;
        private Image m_nextImage;
        public delegate void ReadFinishDelegate(object sender, EventArgs args);
        public event ReadFinishDelegate readFinishedEvent;
        private System.Windows.Forms.Timer timer;//定时器启动

        private System.Timers.Timer wheelTimer;
        object objLock = new object();
        private DateTime startTime;
        private int recordY;
        const double wheelStep = 0.1;
        bool isPress = false;
        bool isExit = false;
        GifBox gifBox;

        public ComicReader()
        {
            //开启双缓冲
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.MouseWheel += ComicReader_MouseWheel;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1;
            timer.Tick += timer_Tick;

            wheelTimer = new System.Timers.Timer();
            wheelTimer.Interval = 35;
            wheelTimer.Elapsed += WheelTimer_Elapsed;
        }      

        public enum PaintStatusEnum
        { 
            PaintLast,
            PaintOne,
            PaintCurrent,
            PaintNext,
            PaintLastNone,
            PaintNextNone
        }

        public List<string> FileUrl
        {
            get;
            set;
        }

        private void AddLoadingGif()
        {
            if(gifBox == null)
                gifBox = new GifBox();

            gifBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            gifBox.Image = Properties.Resources.bycleLoad;
            gifBox.Width = gifBox.Image.Width;
            gifBox.Height = gifBox.Image.Height;
            int x = (this.Width - gifBox.Width) / 2;
            int y = (this.Height - gifBox.Height) / 2;
            gifBox.Location = new Point(x,y);
            this.Controls.Add(gifBox);
        }

        private void RemoveLoadingGif()
        {
            this.Invoke(new Action(() =>
            {
                gifBox.Image = null;
                this.Controls.Remove(gifBox);
            }));           
        }

        public void AddFileToList(object sender, string fileName)//添加文件到文件集合
        {
            if (FileUrl == null)
            {
                FileUrl = new List<string>();
            }

            FileUrl.Add(fileName);
        }

        private void ShowPage(int page)
        {
            pageLabel.Text = (page).ToString() + "/" + (FileUrl.Count).ToString() + "页";
        }

        private void ShowLastTip()
        {
            tishiLabel.Visible = true;
        }

        private void HiddenLastTip()
        {
            tishiLabel.Visible = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var rate = 0.0;
            float show_x = 0.0f;
            int m_imageWidth;
            int m_imageHeight;
            PaintStatusEnum paintStatus;
            RectangleF destRect;
            RectangleF srcRect;
            RectangleF tempRect;
            Graphics gra = e.Graphics;
            base.OnPaint(e);

            paintStatus = PaintStatusEnum.PaintCurrent;
            tempRect = new RectangleF();
            destRect = new RectangleF();
            srcRect = new RectangleF();

            if (m_showImage == null)
                return;

            m_imageWidth = m_showImage.Width;
            m_imageHeight = m_showImage.Height;
            m_clientWidth = this.Width;
            m_clientHeight = this.Height;

            rate = (m_imageHeight * 1.0f) / m_imageWidth;//计算图像宽高比
            rate = rate == 0.0f ? 1.0f : rate;
            //Console.WriteLine("Y的偏移:{0}", m_posY);

            if (m_posY >= m_showImage.Height)//图片高度大于客户界面高度
            {
                m_posY = 0;

                if (m_Page + 1 <= FileUrl.Count - 1)
                {
                    m_showImage = m_nextImage;
                    m_Page++;
                    ShowPage(m_Page + 1);
                }
                else
                {                
                    m_nextImage = null;
                    m_posY = m_showImage.Height;                 
                    return;
                }
            }

            srcRect.X = 0;
            srcRect.Y = m_posY;
            srcRect.Width = m_imageWidth;//图像宽度小于客户区宽度, 取图矩形宽度等于原图宽度

            if (m_posY + m_clientHeight >= m_imageHeight)//第一页已经见底，同时加载当前页和下一页
            {
                paintStatus = PaintStatusEnum.PaintNext;
                srcRect.Height = Convert.ToInt32(m_imageHeight - m_posY);

                if (m_Page + 1 < FileUrl.Count)
                {
                    m_nextImage = Image.FromFile(FileUrl[m_Page + 1]);
                }
                else//已经加载完所有
                {
                    m_nextImage = null;
                    ShowLastTip();

                    if (readFinishedEvent != null)
                    {
                        readFinishedEvent(this, new EventArgs());
                    }
                    
                    return;
                }
 
                tempRect.X = 0;
                tempRect.Y = 0;
                tempRect.Width = m_nextImage.Width;
                tempRect.Height = Convert.ToInt32(tempRect.Width * rate);
            }
            else
            {
                if (m_posY < 0)
                {
                    var pos = Math.Abs(m_posY);
                    HiddenLastTip();
                    paintStatus = PaintStatusEnum.PaintLast;

                    if (m_Page - 1 >= 0)//还有上一页，加载上一页
                    {
                        if (m_nextImage == null || pos >= m_nextImage.Height)//加载完毕，加载上一张图片
                        {
                            if(m_nextImage == null)
                            {
                                m_nextImage = Image.FromFile(FileUrl[m_Page--]);//上一张图片赋值
                            }
                            
                            ShowPage(m_Page);

                            if (Math.Abs(m_posY) >= m_nextImage.Height)//往上翻到头了，而且先画上一张
                            {
                                m_showImage = m_nextImage;

                                if (m_Page - 1 >= 0)
                                    m_nextImage = Image.FromFile(FileUrl[m_Page--]);

                                m_posY = m_showImage.Height - m_clientHeight;
                                srcRect.Y = m_posY;
                                srcRect.Width = m_showImage.Width;
                                srcRect.Height = m_clientHeight;
                                paintStatus = PaintStatusEnum.PaintCurrent;//转为绘制当前             
                            }                          
                        }
                        else//继续加载当前页的上半部分
                        {
                            srcRect.Y = m_nextImage.Height - pos;
                            srcRect.Width = m_nextImage.Width;
                            srcRect.Height = pos;
                            tempRect.Y = 0;
                            tempRect.Width = m_nextImage.Width;
                            tempRect.Height = m_clientHeight - pos;
                        }
                        ShowPage(m_Page);
                    }
                    else
                    {
                        m_posY = 0;
                        srcRect.Height = m_imageHeight;
                        paintStatus = PaintStatusEnum.PaintCurrent;
                    }
                   
                }
                else
                {
                    srcRect.Height = m_imageHeight;
                }
                //GC.Collect();
            }

            if (m_clientWidth >= srcRect.Width)//
            {
                destRect.Width = Convert.ToInt32(srcRect.Width* scale);
                destRect.Height = Convert.ToInt32(srcRect.Height * scale);
   
            }
            else
            {
                destRect.Width = Convert.ToInt32(srcRect.Width * scale);
                destRect.Height = Convert.ToInt32(srcRect.Height * scale);
            }

            show_x = (m_clientWidth - destRect.Width) / 2;//水平居中显示
            destRect.X = show_x;
            destRect.Y = 0;

            Bitmap m_memoryBmp = new Bitmap(m_clientWidth, m_clientHeight);

            using (Graphics memoryGraphic = Graphics.FromImage(m_memoryBmp))
            {
                memoryGraphic.Clear(Color.Black);
                if (paintStatus == PaintStatusEnum.PaintCurrent || paintStatus == PaintStatusEnum.PaintNext)
                {
                    memoryGraphic.DrawImage(m_showImage, destRect, srcRect, GraphicsUnit.Pixel);//绘制到内存图上
        
                    if (paintStatus == PaintStatusEnum.PaintNext)
                    {
                        destRect.Y = Convert.ToInt32(srcRect.Height * scale) + 10;
                        destRect.Width = Convert.ToInt32(tempRect.Width * scale);
                        destRect.Height = Convert.ToInt32(tempRect.Height * scale);
                        memoryGraphic.DrawImage(m_nextImage, destRect, tempRect, GraphicsUnit.Pixel);//第二次绘制
                    }
                }
                else//往上翻页
                {
                    if (paintStatus == PaintStatusEnum.PaintLast)
                    {
                        if (m_nextImage == null)
                        {
                            return;
                        }

                        memoryGraphic.DrawImage(m_nextImage, destRect, srcRect, GraphicsUnit.Pixel);
                        destRect.Y = Convert.ToInt32(srcRect.Height * scale) + 10;
                        destRect.Width = Convert.ToInt32(tempRect.Width * scale);
                        destRect.Height = Convert.ToInt32(tempRect.Height * scale);
                        memoryGraphic.DrawImage(m_showImage, destRect, tempRect, GraphicsUnit.Pixel);
                    }

                }
                                   
                gra.DrawImage(m_memoryBmp, 0, 0);//绘制到屏幕上
                m_memoryBmp.Dispose();
                memoryGraphic.Dispose();
            }

        }

        public void StartRead()
        {
            AddLoadingGif();

            try
            {
                Task task = new Task(() =>
                {
                    while (FileUrl == null || FileUrl.Count < 2)
                    {
                        Thread.Sleep(1);
                    }

                    m_Page = 0;
                    m_showImage = Image.FromFile(FileUrl[m_Page]);

                    if (m_Page + 1 <= FileUrl.Count - 1)
                    {
                        m_nextImage = Image.FromFile(FileUrl[m_Page + 1]);
                    }
                    else
                    {
                        m_nextImage = m_showImage;
                    }

                    RemoveLoadingGif();
                    Invalidate();
                });
                task.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show("StartRead" + ex.Message);
            }
        }

        public bool LoadImage(string path, int currentPage)
        {
            try
            {
                FileUrl = new List<string>();

                if (Directory.Exists(path) == false)
                {
                    MessageBox.Show("无法找到该漫画目录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                var count = Directory.GetFiles(path, "*.jpg").Length;

                if (count == 0)
                {
                    return false;
                }

                for (int i = 0; i < count; i++)
                {
                    FileUrl.Add(path + "\\" + i.ToString() + ".jpg");
                }

                m_Page = currentPage;
                m_showImage = Image.FromFile(FileUrl[m_Page]);

                if (m_Page + 1 <= FileUrl.Count - 1)
                {
                    m_nextImage = Image.FromFile(FileUrl[m_Page + 1]);
                }
                else
                {
                    m_nextImage = m_showImage;
                }

                this.Text = FileUrl[m_Page].Substring(0, FileUrl[m_Page].LastIndexOf("\\"));
                Init();

                foreach (ToolStripMenuItem t in menuTool.Items)
                {
                    if (t.Text.Equals("历史(&H)"))
                        t.DropDownItems.Add(FileUrl[m_Page]);
                }

                Invalidate();
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("LoadImage ：{0}", ex.Message);
            }

            return false;
        }

        void Init()
        {
            pageLabel.Text = (m_Page+1).ToString() + "/" + (FileUrl.Count).ToString() + "页";
        }

        private void WheelTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            wheelTimer.Stop();

            this.Invoke(new Action(() =>
            {
                if (isAdd)
                {
                    UpScroll(sum);
                }
                else
                {
                    DownScroll(sum);
                }

            }));
            
        }

        void ComicReader_MouseWheel(object sender, MouseEventArgs e)
        {
            wheelTimer.Stop();

            lock (objLock)
            {
                int movePos = Math.Abs(e.Delta);
                
                Console.WriteLine("滚动长度:{0}", sum);

                if (e.Delta < 0)
                {
                    sum = sum + movePos;
                    isAdd = true;                  
                }
                else
                {
                    sum = movePos;
                    isAdd = false;
                }

                wheelTimer.Start();
            }
            
            
        }

        void UpScroll(int offset)
        { 
            time = 0;
            sum = offset;
            isAdd = true;       
            timer.Start();
        }

        void DownScroll(int offset)
        { 
            time = 0;
            sum = offset;
            isAdd = false;           
            timer.Start();
        }

        void SineEraseIn(double t)
        {
            double y = Math.Sin(t * Math.PI / 2);
        }

        private double EaseOutQuad(double x)
        {
            return 1 - Math.Pow(1 - x, 4);
        }

        private double CalIncrease()
        {
            double dt = (EaseOutQuad(time + wheelStep) - EaseOutQuad(time))*sum;        

            if(dt < 0)
            {
                return 0;
            }
           return dt;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("开始绘制");
            lock (objLock)
            {
                double increase = 0.0f;//increase 增量
                increase = CalIncrease();

                if (isAdd)
                {
                    m_posY = m_posY + Convert.ToInt32(increase);//计算下个顶点坐标的位置
                }
                else
                {
                    m_posY = m_posY - Convert.ToInt32(increase);
                }

                //Console.WriteLine("increase：{0}, time{1}", increase, time);

                if (Math.Abs(increase) < 1)

                {
                    timer.Stop();
                    sum = 0;
                }

                time += wheelStep;
                Invalidate();
            }
            
        }

       

        private void OpenFile()
        {
            OpenFileDialog openFile = new OpenFileDialog();

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string path = openFile.FileName;
                int page = Convert.ToInt32(openFile.SafeFileName.Substring(0, openFile.SafeFileName.IndexOf(".")));
                LoadImage(path.Substring(0, path.LastIndexOf(@"\")), page);
            }
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void FullScreen()
        {
            if (fullScreenTool.Checked == false)
            {
                this.SetVisibleCore(false);
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.SetVisibleCore(true);
                pageLabel.Visible = false;
                menuTool.Visible = false;
                fullScreenTool.Checked = true;            
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                this.Width = 1280;
                this.Height = 720;
                pageLabel.Visible = true;
                fullScreenTool.Checked = false;
                menuTool.Visible = true;
            }
            
        }

        private void 全屏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FullScreen();
        }

        private void skinButton1_Click(object sender, EventArgs e)
        {
            scale = scale + 0.2;
            Invalidate();
        }

        private void prograssBar_Scroll(object sender, EventArgs e)
        {
            /*m_Page = prograssBar.Value;
            m_showImage = Image.FromFile(FileUrl[m_Page]);
            m_posY = 0;
            Invalidate();*/
        }

        private void ComicReader_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up: DownScroll(120); break;
                case Keys.Down: UpScroll(120); break;
                case Keys.Add: Enlarge(); break;
                case Keys.Subtract: Narrow(); break;
                case Keys.Enter:FullScreen(); break;
                
            }
        }

        private void ComicReader_Click(object sender, EventArgs e)
        {
            
        }

        private void 放大ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Enlarge();
        }

     
        private void 缩小ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Narrow();
        }

        private void Enlarge()
        {
            scale = scale + 0.2;
            Invalidate();
        }

        private void Narrow()
        {
            scale = scale - 0.2;
            Invalidate();
        }

        private void 适应宽度ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scale = m_clientWidth > m_showImage.Width ? m_clientWidth / m_showImage.Width : m_showImage.Width / m_clientWidth;
            autoWidth.Checked = !autoWidth.Checked;
            Invalidate();
        }

        private void 原始比例IToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scale = 1.0;
            Invalidate();
        }

        private void historyMenu_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int index = FileUrl.FindIndex(a=>a == e.ClickedItem.Text);

            if (index == -1)
            { 
                
            }

            if (m_Page == index)
            {
                return;
            }

            m_Page = index;
            m_showImage = Image.FromFile(FileUrl[m_Page]);
            Invalidate();
        }

        private void 清除缓存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteFiles("temp\\");
            MessageBox.Show("缓存清除成功");
        }

        private void 关于BToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            comicDownLoad.Properties.AboutBox about = new Properties.AboutBox();
            about.Show();
        }

        public void DeleteFiles(string str)
        {
            try
            {
                DirectoryInfo fatherFolder = new DirectoryInfo(str);
                //删除当前文件夹内文件
                FileInfo[] files = fatherFolder.GetFiles();
                foreach (FileInfo file in files)
                {
                    //string fileName = file.FullName.Substring((file.FullName.LastIndexOf("\\") + 1), file.FullName.Length - file.FullName.LastIndexOf("\\") - 1);
                    string fileName = file.Name;
                    try
                    {
                        if (!fileName.Equals("index.dat"))
                        {
                            File.Delete(file.FullName);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                //递归删除子文件夹内文件
                foreach (DirectoryInfo childFolder in fatherFolder.GetDirectories())
                {
                    DeleteFiles(childFolder.FullName);
                }
            }
            catch(Exception ex)
            {

            }

        }

        private void openDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.Description = "打开漫画保存目录";
            
            if (folder.ShowDialog() == DialogResult.OK)
            {
                string path = folder.SelectedPath;
                DirectoryInfo info = new DirectoryInfo(path);

                foreach (var i in info.GetFiles())
                {
                    Console.WriteLine(i.FullName);
                }
            }
        }


        private void ComicReader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                recordY = e.Y;
                isPress = true;
                startTime = DateTime.Now;
            }
            
        }

        private void ComicReader_MouseUp(object sender, MouseEventArgs e)
        {
            if (isPress)
            {
                int movePos = e.Y - recordY;

                if (movePos < 0)
                {
                    UpScroll(Math.Abs(movePos));
                }
                else
                {
                    DownScroll(Math.Abs(movePos));
                }

                isPress = false;
            }           
           
        }

        private void addMarkItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem t in menuTool.Items)
            {
                if (t.Text.Equals("书签(&B)"))
                    t.DropDownItems.Add(FileUrl[m_Page]);
            }
        }

        private void bookMenulItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)//书签跳转
        {

        }

        private void progressBar_MouseLeave(object sender, EventArgs e)
        {
            this.Focus();
        }

        private void ComicReader_Resize(object sender, EventArgs e)
        {
            tishiLabel.Left = (this.Width - tishiLabel.Width)/ 2;

            if(gifBox != null)
            {
                gifBox.Left = (this.Width - gifBox.Width) / 2;
                gifBox.Top = (this.Height - gifBox.Height) / 2;
            }
           
        }

        private void ComicReader_FormClosing(object sender, FormClosingEventArgs e)//关闭窗口
        {
            isExit = true;
        }

        private void ComicReader_Scroll(object sender, ScrollEventArgs e)
        {
            Console.WriteLine("滚动了");
        }
    }
}
