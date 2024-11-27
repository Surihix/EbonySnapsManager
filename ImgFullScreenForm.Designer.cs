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
            this.ImgPicBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.ImgPicBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ImgPicBox
            // 
            this.ImgPicBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.ImgPicBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImgPicBox.Location = new System.Drawing.Point(0, 0);
            this.ImgPicBox.Name = "ImgPicBox";
            this.ImgPicBox.Size = new System.Drawing.Size(800, 450);
            this.ImgPicBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.ImgPicBox.TabIndex = 0;
            this.ImgPicBox.TabStop = false;
            this.ImgPicBox.DoubleClick += new System.EventHandler(this.ImgPicBox_DoubleClick);
            // 
            // ImgFullScreenForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ImgPicBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimizeBox = false;
            this.Name = "ImgFullScreenForm";
            this.Text = "ImgFullScreen";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.ImgFullScreenForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ImgFullScreenForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.ImgPicBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox ImgPicBox;
    }
}