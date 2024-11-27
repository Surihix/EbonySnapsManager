using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EbonySnapsManager
{
    public partial class ImgFullScreenForm : Form
    {
        public static byte[] ImgData = new byte[] { };

        public ImgFullScreenForm()
        {
            InitializeComponent();
        }

        private void ImgFullScreenForm_Load(object sender, EventArgs e)
        {
            using (var fullScreenImgStream = new MemoryStream())
            {
                fullScreenImgStream.Write(ImgData, 0, ImgData.Length);
                fullScreenImgStream.Seek(0, SeekOrigin.Begin);

                Image img = Image.FromStream(fullScreenImgStream);
                ImgPicBox.Image = img;
                Size = img.Size;
            }
        }

        private void ImgPicBox_DoubleClick(object sender, EventArgs e)
        {
            Close();
        }

        private void ImgFullScreenForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }
    }
}