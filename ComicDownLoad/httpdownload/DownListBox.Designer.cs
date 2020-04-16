namespace comicDownLoad
{
    partial class DownListBox
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.titleLabel = new System.Windows.Forms.Label();
            this.downProgressBar = new CCWin.SkinControl.SkinProgressBar();
            this.checkBtn = new CCWin.SkinControl.SkinCheckBox();
            this.readBtn = new CCWin.SkinControl.SkinButton();
            this.refreshBtn = new CCWin.SkinControl.SkinButton();
            this.openBtn = new CCWin.SkinControl.SkinButton();
            this.deleteBtn = new CCWin.SkinControl.SkinButton();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.titleLabel.Location = new System.Drawing.Point(31, 5);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(53, 12);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "漫画名称";
            this.titleLabel.Click += new System.EventHandler(this.titleLabel_Click);
            // 
            // downProgressBar
            // 
            this.downProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.downProgressBar.Back = null;
            this.downProgressBar.BackColor = System.Drawing.Color.Transparent;
            this.downProgressBar.BarBack = null;
            this.downProgressBar.BarRadius = 8;
            this.downProgressBar.BarRadiusStyle = CCWin.SkinClass.RoundStyle.All;
            this.downProgressBar.ForeColor = System.Drawing.Color.Black;
            this.downProgressBar.Location = new System.Drawing.Point(5, 22);
            this.downProgressBar.Name = "downProgressBar";
            this.downProgressBar.Radius = 8;
            this.downProgressBar.RadiusStyle = CCWin.SkinClass.RoundStyle.All;
            this.downProgressBar.Size = new System.Drawing.Size(334, 16);
            this.downProgressBar.TabIndex = 4;
            this.downProgressBar.TextFormat = CCWin.SkinControl.SkinProgressBar.TxtFormat.Proportion;
            this.downProgressBar.TrackBack = System.Drawing.Color.White;
            this.downProgressBar.TrackFore = System.Drawing.Color.MediumTurquoise;
            this.downProgressBar.Value = 50;
            // 
            // checkBtn
            // 
            this.checkBtn.AutoSize = true;
            this.checkBtn.BackColor = System.Drawing.Color.Transparent;
            this.checkBtn.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.checkBtn.DownBack = null;
            this.checkBtn.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.checkBtn.LightEffect = false;
            this.checkBtn.LightEffectWidth = 0;
            this.checkBtn.Location = new System.Drawing.Point(10, 3);
            this.checkBtn.MouseBack = null;
            this.checkBtn.Name = "checkBtn";
            this.checkBtn.NormlBack = null;
            this.checkBtn.SelectedDownBack = null;
            this.checkBtn.SelectedMouseBack = null;
            this.checkBtn.SelectedNormlBack = null;
            this.checkBtn.Size = new System.Drawing.Size(15, 14);
            this.checkBtn.TabIndex = 6;
            this.checkBtn.UseVisualStyleBackColor = false;
            // 
            // readBtn
            // 
            this.readBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.readBtn.BackColor = System.Drawing.Color.Transparent;
            this.readBtn.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.readBtn.DownBack = global::comicDownLoad.Properties.Resources.readSmall;
            this.readBtn.DrawType = CCWin.SkinControl.DrawStyle.Img;
            this.readBtn.Location = new System.Drawing.Point(375, 14);
            this.readBtn.MouseBack = global::comicDownLoad.Properties.Resources.redSmallHover;
            this.readBtn.Name = "readBtn";
            this.readBtn.NormlBack = global::comicDownLoad.Properties.Resources.readSmall;
            this.readBtn.Size = new System.Drawing.Size(24, 24);
            this.readBtn.TabIndex = 8;
            this.readBtn.UseVisualStyleBackColor = false;
            this.readBtn.Click += new System.EventHandler(this.readBtn_Click);
            // 
            // refreshBtn
            // 
            this.refreshBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.refreshBtn.BackColor = System.Drawing.Color.Transparent;
            this.refreshBtn.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.refreshBtn.DownBack = global::comicDownLoad.Properties.Resources.refresh;
            this.refreshBtn.DrawType = CCWin.SkinControl.DrawStyle.Img;
            this.refreshBtn.Location = new System.Drawing.Point(345, 14);
            this.refreshBtn.MouseBack = global::comicDownLoad.Properties.Resources.refresh_green;
            this.refreshBtn.Name = "refreshBtn";
            this.refreshBtn.NormlBack = global::comicDownLoad.Properties.Resources.refresh;
            this.refreshBtn.Size = new System.Drawing.Size(24, 24);
            this.refreshBtn.TabIndex = 7;
            this.refreshBtn.UseVisualStyleBackColor = false;
            this.refreshBtn.Click += new System.EventHandler(this.refreshBtn_Click);
            // 
            // openBtn
            // 
            this.openBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.openBtn.BackColor = System.Drawing.Color.Transparent;
            this.openBtn.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.openBtn.DownBack = global::comicDownLoad.Properties.Resources.file_hover;
            this.openBtn.DrawType = CCWin.SkinControl.DrawStyle.Img;
            this.openBtn.Location = new System.Drawing.Point(403, 14);
            this.openBtn.MouseBack = global::comicDownLoad.Properties.Resources.file_hover;
            this.openBtn.Name = "openBtn";
            this.openBtn.NormlBack = global::comicDownLoad.Properties.Resources.file;
            this.openBtn.Size = new System.Drawing.Size(38, 24);
            this.openBtn.TabIndex = 5;
            this.openBtn.UseVisualStyleBackColor = false;
            this.openBtn.Click += new System.EventHandler(this.openBtn_Click);
            // 
            // deleteBtn
            // 
            this.deleteBtn.BackColor = System.Drawing.Color.Transparent;
            this.deleteBtn.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.deleteBtn.DownBack = global::comicDownLoad.Properties.Resources.delete;
            this.deleteBtn.DrawType = CCWin.SkinControl.DrawStyle.Img;
            this.deleteBtn.Image = global::comicDownLoad.Properties.Resources.delete;
            this.deleteBtn.Location = new System.Drawing.Point(447, 14);
            this.deleteBtn.MouseBack = global::comicDownLoad.Properties.Resources.delete;
            this.deleteBtn.Name = "deleteBtn";
            this.deleteBtn.NormlBack = null;
            this.deleteBtn.Size = new System.Drawing.Size(26, 23);
            this.deleteBtn.TabIndex = 9;
            this.deleteBtn.UseVisualStyleBackColor = false;
            this.deleteBtn.Click += new System.EventHandler(this.deleteBtn_Click);
            // 
            // DownListBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.deleteBtn);
            this.Controls.Add(this.readBtn);
            this.Controls.Add(this.refreshBtn);
            this.Controls.Add(this.checkBtn);
            this.Controls.Add(this.openBtn);
            this.Controls.Add(this.downProgressBar);
            this.Controls.Add(this.titleLabel);
            this.Name = "DownListBox";
            this.Size = new System.Drawing.Size(476, 41);
            this.Click += new System.EventHandler(this.DownListBox_Click);
            this.Leave += new System.EventHandler(this.DownListBox_Leave);
            this.MouseEnter += new System.EventHandler(this.DownListBox_MouseEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label titleLabel;
        private CCWin.SkinControl.SkinProgressBar downProgressBar;
        private CCWin.SkinControl.SkinButton openBtn;
        private CCWin.SkinControl.SkinCheckBox checkBtn;
        private CCWin.SkinControl.SkinButton refreshBtn;
        private CCWin.SkinControl.SkinButton readBtn;
        private CCWin.SkinControl.SkinButton deleteBtn;
    }
}
