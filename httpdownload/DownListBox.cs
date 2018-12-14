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
                checkBtn.Checked = IsChecked;

                if (checkBtn.Checked)
                {
                    checkBtn.Visible = true;
                }
                else
                {
                    checkBtn.Visible = false;
                }
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
            set { m_Page = value; }
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
                downProgressBar.Value = value;
                m_Page = value;                                                 
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

        private void refreshBtn_Click(object sender, EventArgs e)
        {

        }

        private void readBtn_Click(object sender, EventArgs e)
        {
            ComicReader reader = new ComicReader();
            if (FilePath == null || FilePath == "")
                return;
            reader.LoadImage(FilePath, 0);
            reader.Show();
        }        
        
    }
}
