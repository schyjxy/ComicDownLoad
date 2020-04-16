using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace comicDownLoad
{
    public partial class DownListBox : UserControl
    {
        public DownListBox()
        {
            InitializeComponent();
        }

        public delegate void ResumeDownLoadEventHandler(object sender, EventArgs args);
        public delegate void DeleteEventHandler(object sender, EventArgs args);
        public event ResumeDownLoadEventHandler resumeDownLoadEvent;
        public event DeleteEventHandler deleteEvent;
        private int m_comicCount;
        private int m_Page;
        private bool m_downFinished;

        public string Title//漫画名称
        {
            get
            {
                return titleLabel.Text;
            }

            set
            {
                titleLabel.Text = value;
            }
        }

        public bool IsChecked
        {
            get { return checkBtn.Checked; }
            set
            {
                checkBtn.Checked = value;
            }
        }

        public string FilePath
        {
            get;
            set;
        }

        public bool DownFinished
        {
            get
            {
                return m_downFinished;
            }
            set
            {
                m_downFinished = value;
                if (m_downFinished)
                {
                    openBtn.Enabled = true;
                }
                else 
                {
                    openBtn.Enabled = false;
                }
            }
        }

        public int CurrentPage//当前下载到第几页
        {
            get { return m_Page; }
            set 
            { 
                m_Page = value;
                downProgressBar.Value = m_Page;
            }
        }

        /// <summary>
        /// 设置最大页数
        /// </summary>
        /// <param name="num"></param>
        public void SetMaxPage(int num)
        {
            m_comicCount = num;

            if (downProgressBar.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    downProgressBar.Value = 0;
                    downProgressBar.Maximum = num;
                }));
            }
            else
            {
                downProgressBar.Value = 0;
                downProgressBar.Maximum = num;
            }
           
        }

        public int ProgressVal
        {
            get 
            { 
               return downProgressBar.Value;                
            }
            set 
            {
                if (downProgressBar.Maximum >= value)
                {
                    downProgressBar.Value = value;
                    m_Page = value;
                }
                                                                 
            }
        }

        public int Pages
        {
            get { return m_comicCount; }

            set
            {
                m_comicCount = value;
            }               
        }

        private void openBtn_Click(object sender, EventArgs e)
        {
            if (FilePath != null && System.IO.Directory.Exists(FilePath))
            {
                System.Diagnostics.Process.Start("explorer.exe", FilePath);
            }
            else
            {
                MessageBox.Show("不存在该目录");
            }
        }

        private void refreshBtn_Click(object sender, EventArgs e)//继续下载
        {
            if (resumeDownLoadEvent != null)
            {
                resumeDownLoadEvent(this, new EventArgs());
            }

        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            if (deleteEvent != null)
            {
                deleteEvent(this, new EventArgs());
            }
        }      

        private void readBtn_Click(object sender, EventArgs e)
        {
            ComicReader reader = new ComicReader();
            if (FilePath == null || FilePath == "")
                return;
            if(reader.LoadImage(FilePath, 0))
                reader.Show();
        }

        private void DownListBox_Click(object sender, EventArgs e)
        {
            SetCheckBox();
        }

        private void SetCheckBox()
        {
            if (this.IsChecked)
            {
                checkBtn.Checked = false;
            }
            else
                checkBtn.Checked = true;
        }

        private void titleLabel_Click(object sender, EventArgs e)
        {
            SetCheckBox();
        }

        private void DownListBox_Leave(object sender, EventArgs e)
        {
            
        }

        private void DownListBox_MouseEnter(object sender, EventArgs e)
        {
            
        }
       
        
    }
}
