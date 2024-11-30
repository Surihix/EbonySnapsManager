namespace EbonySnapsManager
{
    partial class ImgFullScreenForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImgFullScreenForm));
            this.ImgPicBox = new System.Windows.Forms.PictureBox();
            this.ImgPicBoxContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SaveImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.ImgPicBox)).BeginInit();
            this.ImgPicBoxContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ImgPicBox
            // 
            this.ImgPicBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
            this.ImgPicBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImgPicBox.Location = new System.Drawing.Point(0, 0);
            this.ImgPicBox.Name = "ImgPicBox";
            this.ImgPicBox.Size = new System.Drawing.Size(800, 450);
            this.ImgPicBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ImgPicBox.TabIndex = 0;
            this.ImgPicBox.TabStop = false;
            this.ImgPicBox.DoubleClick += new System.EventHandler(this.ImgPicBox_DoubleClick);
            // 
            // ImgPicBoxContextMenuStrip
            // 
            this.ImgPicBoxContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SaveImageToolStripMenuItem});
            this.ImgPicBoxContextMenuStrip.Name = "ImgPicBoxContextMenuStrip";
            this.ImgPicBoxContextMenuStrip.Size = new System.Drawing.Size(135, 26);
            // 
            // SaveImageToolStripMenuItem
            // 
            this.SaveImageToolStripMenuItem.Name = "SaveImageToolStripMenuItem";
            this.SaveImageToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.SaveImageToolStripMenuItem.Text = "Save Image";
            this.SaveImageToolStripMenuItem.Click += new System.EventHandler(this.SaveImageToolStripMenuItem_Click);
            // 
            // ImgFullScreenForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ImgPicBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "ImgFullScreenForm";
            this.Text = "ImgFullScreen";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.ImgFullScreenForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ImgFullScreenForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.ImgPicBox)).EndInit();
            this.ImgPicBoxContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox ImgPicBox;
        private System.Windows.Forms.ContextMenuStrip ImgPicBoxContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem SaveImageToolStripMenuItem;
    }
}