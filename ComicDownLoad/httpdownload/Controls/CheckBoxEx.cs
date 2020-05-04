using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace comicDownLoad.Controls
{
    public partial class CheckBoxEx : UserControl
    {
        public CheckBoxEx()
        {
            InitializeComponent();
        }

        private bool m_isChecked = false;
        private string m_text;

        public bool Checked
        {
            get { return m_isChecked; }
            set
            {           
                m_isChecked = value;
                SetBackGround();
            }
        }
        public string Text
        {
            get { return label1.Text; }
            set
            {
                SetToopTip(value);
                m_text = value;

                if (value.Length > 12)
                {
                    label1.Text = value.Substring(0, 12) + "...";
                }
                else
                {
                    label1.Text = value;
                }
                
            }
        }

        public string FullText
        {
            get { return m_text; }
        }

        private void SetBackGround()
        {
            if (m_isChecked)
            {
                this.BackColor = Color.FromArgb(0,162,255);
            }
            else
            {
                this.BackColor = Color.White;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            SolidBrush solidBrush = new SolidBrush(Color.Silver);
            Pen pen = new Pen(solidBrush, 1);
            g.DrawRectangle(pen, new Rectangle(0, 0, this.Width-1, this.Height-1));
            base.OnPaint(e);
        }

        private void SetToopTip(string content)
        {
            ToolTip tipText = new ToolTip();
            tipText.AutoPopDelay = 5000;
            tipText.InitialDelay = 500;
            tipText.ShowAlways = true;
            tipText.ReshowDelay = 500;
            tipText.SetToolTip(this, content);
            tipText.SetToolTip(label1, content);
        }

        private void CheckBoxEx_Click(object sender, EventArgs e)
        {
            m_isChecked = !m_isChecked;
            SetBackGround();
        }

        private void CheckBoxEx_Load(object sender, EventArgs e)
        {
            
        }

        private void label1_Click(object sender, EventArgs e)
        {
            m_isChecked = !m_isChecked;
            SetBackGround();
        }
    }
}
