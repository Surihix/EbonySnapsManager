using MahApps.Metro.Controls;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace EbonySnapsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static Dictionary<string, string> SnapshotFilesInDirDict = new Dictionary<string, string>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            AppStatusBarTxt.Text = "App launched!";
        }


        private void LoadSnapshotBtn_Click(object sender, RoutedEventArgs e)
        {
            var snapshotSelect = new OpenFileDialog
            {
                Title = "Select a FFXV Snapshot file",
                Filter = "Snapshot file (*.ss)|*.ss"
            };

            if (snapshotSelect.ShowDialog() == true)
            {
                SnapViewerImgBox.Source = null;
                DrawOnImgBox(SnapshotHelpers.GetImageDataFromSnapshotFile(snapshotSelect.FileName), 1, Path.GetFileName(snapshotSelect.FileName));
            }
        }


        private void LoadSnapshotDirectoryBtn_Click(object sender, RoutedEventArgs e)
        {
            var snapshotDirSelect = new VistaFolderBrowserDialog()
            {
                Description = "Select FFXV snapshot directory",
                UseDescriptionForTitle = true
            };

            if (snapshotDirSelect.ShowDialog() == true)
            {
                var snapshotDir = Directory.GetFiles(snapshotDirSelect.SelectedPath, "*.ss", SearchOption.TopDirectoryOnly);

                if (snapshotDir.Length == 0)
                {
                    MessageBox.Show("Unable to find FFXV snapshot files in the selected folder", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    foreach (var ssFile in snapshotDir)
                    {
                        var ssFileName = Path.GetFileName(ssFile);
                        SnapshotFilesInDirDict.Add(ssFileName, ssFile);
                        SnapshotListbox.Items.Add(ssFileName);
                    }
                }
            }
        }


        private void SnapshotListbox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var imgFile = string.Empty;

            if (File.Exists(SnapshotFilesInDirDict[(string)SnapshotListbox.SelectedItem]))
            {
                imgFile = SnapshotFilesInDirDict[(string)SnapshotListbox.SelectedItem];
                DrawOnImgBox(SnapshotHelpers.GetImageDataFromSnapshotFile(imgFile), 1, Path.GetFileName(imgFile));
            }
        }


        private void LoadImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var imgSelect = new OpenFileDialog
            {
                Title = "Select an image file",
                Filter = "Image file (*.jpg;*.png)|*.jpg;*.png"
            };

            if (imgSelect.ShowDialog() == true)
            {

            }
        }


        private void AddNewSnapshotBtn_Click(object sender, RoutedEventArgs e)
        {
            var saveFileSelect = new OpenFileDialog()
            {
                Title = "Select a valid FFXV save file",
                Filter = "FFXV save file (gameplay0.save)|gameplay0.save"
            };

            if (saveFileSelect.ShowDialog() == true)
            {
                var snapshotFileSelect = new OpenFileDialog()
                {
                    Title = "Select a snapshotlink file",
                    Filter = "snapshotlink file (snapshotlink.sl)|snapshotlink.sl"
                };

                if (snapshotFileSelect.ShowDialog() == true)
                {

                }
            }
        }


        private void ReplaceSnapshotBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ReplaceSnapshot option", "Title", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void ImportSnapsFromSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ImportSnapsFromSave option", "Title", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void AdjustSnapshotBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("AdjustSnapshot option", "Title", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void DrawOnImgBox(byte[] imgData, int imgBoxId, string imgFileName)
        {
            var bitmap = new BitmapImage();

            using (var imgStream = new MemoryStream())
            {
                imgStream.Write(imgData, 0, imgData.Length);
                imgStream.Seek(0, SeekOrigin.Begin);

                bitmap.BeginInit();
                bitmap.StreamSource = imgStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }

            if (imgBoxId == 1)
            {
                SnapViewerImgBox.Source = bitmap;
            }
            else
            {
                SnapToolsImgBox.Source = bitmap;
            }

            AppStatusBarTxt.Text = $"Loaded {imgFileName}";
        }
    }
}