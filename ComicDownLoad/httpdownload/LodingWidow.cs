using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace comicDownLoad
{
    public partial class LodingWidow : Form
    {
        public LodingWidow()
        {
            InitializeComponent();
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ForeColor = System.Drawing.SystemColors.Desktop;
            this.BackColor = System.Drawing.Color.White;
            this.Opacity = 1;
        }

        private void SetWindowTransparent(byte bAlpha)
        {
            try
            {
                WinAPI.SetWindowLong(this.Handle, (int)WinAPI.WindowStyle.GWL_EXSTYLE, WinAPI.GetWindowLong(this.Handle, (int)WinAPI.WindowStyle.GWL_EXSTYLE) | (uint)WinAPI.ExWindowStyle.WS_EX_LAYERED);
                WinAPI.SetLayeredWindowAttributes(this.Handle, 0, bAlpha, WinAPI.LWA_COLORKEY | WinAPI.LWA_ALPHA);
            }
            catch
            {

            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Parent = WinAPI.GetDesktopWindow();
                cp.ExStyle = 0x00000080 | 0x00000008;  //WS_EX_TOOLWINDOW | WS_EX_TOPMOST   
                return cp;
            }
        }

        private void LodingWidow_Load(object sender, EventArgs e)
        {
            //this.SetWindowTransparent(100);
        }
    }
    
    public class WinAPI
    {
        [DllImport("user32.dll")]
        public extern static IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public extern static bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public static uint LWA_COLORKEY = 0x00000001;
        public static uint LWA_ALPHA = 0x00000002;

        [DllImport("user32.dll")]
        public extern static uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        public extern static uint GetWindowLong(IntPtr hwnd, int nIndex);

        public enum WindowStyle : int { GWL_EXSTYLE = -20 }
        public enum ExWindowStyle : uint { WS_EX_LAYERED = 0x00080000 }
    }
}
