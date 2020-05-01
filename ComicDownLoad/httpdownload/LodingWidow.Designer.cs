namespace comicDownLoad
{
    partial class LodingWidow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gifBox1 = new CCWin.SkinControl.GifBox();
            this.SuspendLayout();
            // 
            // gifBox1
            // 
            this.gifBox1.BorderColor = System.Drawing.Color.Transparent;
            this.gifBox1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.gifBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gifBox1.Image = global::comicDownLoad.Properties.Resources.loading;
            this.gifBox1.Location = new System.Drawing.Point(0, 0);
            this.gifBox1.Name = "gifBox1";
            this.gifBox1.Size = new System.Drawing.Size(282, 174);
            this.gifBox1.TabIndex = 0;
            this.gifBox1.Text = "gifBox1";
            // 
            // LodingWidow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 174);
            this.Controls.Add(this.gifBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LodingWidow";
            this.Opacity = 0.1D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LodingWidow";
            this.Load += new System.EventHandler(this.LodingWidow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.GifBox gifBox1;
    }
}