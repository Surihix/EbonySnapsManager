using EbonySnapsManager.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace EbonySnapsManager
{
    public partial class ImgFullScreenForm : Form
    {
        public static byte[] ImgData = new byte[] { };
        public static bool IsSnapshotFile;
        public static string CurrentSSName;

        public ImgFullScreenForm()
        {
            InitializeComponent();

            if (IsSnapshotFile)
            {
                ImgPicBox.ContextMenuStrip = ImgPicBoxContextMenuStrip;
            }
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

        private void SaveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog()
            {
                Title = "Save Image file",
                FileName = $"{Path.GetFileNameWithoutExtension(CurrentSSName)}",
                Filter = "All files (*.*)|*.*",
                OverwritePrompt = true,
                RestoreDirectory = true
            };

            if (sfd.ShowDialog() == DialogResult.OK && sfd.FileName != null)
            {
                var outImgFile = SnapshotHelpers.SaveImgDataToFile(sfd.FileName, Path.GetDirectoryName(sfd.FileName), ImgData);

                System.Windows.MessageBox.Show($"Saved image file \"{Path.GetFileName(outImgFile)}\"", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}