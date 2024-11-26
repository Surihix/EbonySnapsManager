using MahApps.Metro.Controls;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EbonySnapsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private static readonly Dictionary<string, string> SnapshotFilesInDirDict = new Dictionary<string, string>();
        private readonly AppViewModel ViewModelInstance = new AppViewModel();

        private static TextBlock AppStatusTxtBlockComp { get; set; }
        private static ListBox SnapshotListBoxComp { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModelInstance;
            ViewModelInstance.IsUIenabled = true;
            ViewModelInstance.StatusBarTxt = "App launched!";
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
                ViewModelInstance.BitmapSrc0 = null;
                DrawOnImgBox(SnapshotHelpers.GetImgDataFromSnapshotFile(snapshotSelect.FileName), 0, Path.GetFileName(snapshotSelect.FileName));
            }
        }


        private void LoadSnapshotDirectoryBtn_Click(object sender, RoutedEventArgs e)
        {
            var snapshotDirSelect = new VistaFolderBrowserDialog()
            {
                Description = "Select a FFXV snapshot directory",
                UseDescriptionForTitle = true
            };

            if (snapshotDirSelect.ShowDialog() == true)
            {
                try
                {
                    var snapshotDir = Directory.GetFiles(snapshotDirSelect.SelectedPath, "*.ss", SearchOption.TopDirectoryOnly);

                    if (snapshotDir.Length == 0)
                    {
                        MessageBox.Show("Unable to find valid FFXV snapshot files in the selected folder", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        ViewModelInstance.StatusBarTxt = "Loading snapshot files....";
                        ViewModelInstance.IsUIenabled = false;
                        AppStatusTxtBlockComp = (TextBlock)FindName("AppStatusBarTxtBlock");
                        SnapshotListBoxComp = (ListBox)FindName("SnapshotListbox");

                        System.Threading.Tasks.Task.Run(() =>
                        {
                            try
                            {
                                SnapshotListBoxComp.BeginInvoke(new Action(() => SnapshotListBoxComp.Items.Clear()));
                                SnapshotFilesInDirDict.Clear();

                                foreach (var ssFile in snapshotDir)
                                {
                                    var ssFileName = Path.GetFileName(ssFile);
                                    SnapshotFilesInDirDict.Add(ssFileName, ssFile);
                                    SnapshotListBoxComp.BeginInvoke(new Action(() => SnapshotListBoxComp.Items.Add(ssFileName)));
                                }
                            }
                            finally
                            {
                                Dispatcher.BeginInvoke(new Action(() => ViewModelInstance.IsUIenabled = true));
                                AppStatusTxtBlockComp.BeginInvoke(new Action(() => AppStatusTxtBlockComp.Text = "Loaded snapshot files from directory"));
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ViewModelInstance.IsUIenabled = true;
                }
            }
        }


        private void SnapshotListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SnapshotListbox.SelectedItem != null)
            {
                if (File.Exists(SnapshotFilesInDirDict[(string)SnapshotListbox.SelectedItem]))
                {
                    var imgFile = SnapshotFilesInDirDict[(string)SnapshotListbox.SelectedItem];
                    DrawOnImgBox(SnapshotHelpers.GetImgDataFromSnapshotFile(imgFile), 0, Path.GetFileName(imgFile));
                }
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
                var imgFile = imgSelect.FileName;
                DrawOnImgBox(File.ReadAllBytes(imgFile), 1, Path.GetFileName(imgFile));
            }
        }


        private void AddNewSnapshotBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModelInstance.BitmapSrc1 == null)
            {
                MessageBox.Show("A valid image file is not selected. Please load an image file into the panel before using this option", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var saveFileSelect = new OpenFileDialog()
                {
                    Title = "Select a FFXV save file",
                    Filter = "FFXV save file (gameplay0.save)|gameplay0.save"
                };

                if (saveFileSelect.ShowDialog() == true)
                {
                    var snapshotlinkFileSelect = new OpenFileDialog()
                    {
                        Title = "Select a snapshotlink file",
                        Filter = "snapshotlink file (snapshotlink.sl)|snapshotlink.sl"
                    };

                    if (snapshotlinkFileSelect.ShowDialog() == true)
                    {

                    }
                }
            }
        }


        private void ReplaceSnapshotBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModelInstance.BitmapSrc1 == null)
            {
                MessageBox.Show("A valid image file is not selected. Please load an image file into the panel before using this option", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var snapshotFileSelect = new OpenFileDialog()
                {
                    Title = "Select a FFXV Snapshot file",
                    Filter = "Snapshot file (*.ss)|*.ss"
                };

                if (snapshotFileSelect.ShowDialog() == true)
                {

                }
            }
        }


        private void ImportSnapsFromSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var saveFileSelect = new OpenFileDialog()
            {
                Title = "Select a FFXV save file",
                Filter = "FFXV save file (gameplay0.save)|gameplay0.save"
            };

            if (saveFileSelect.ShowDialog() == true)
            {
                var snapshotlinkFileSelect = new OpenFileDialog()
                {
                    Title = "Select a snapshotlink file",
                    Filter = "snapshotlink file (snapshotlink.sl)|snapshotlink.sl"
                };

                if (snapshotlinkFileSelect.ShowDialog() == true)
                {

                }
            }
        }


        private void AdjustSnapshotBtn_Click(object sender, RoutedEventArgs e)
        {
            var saveFileSelect = new OpenFileDialog()
            {
                Title = "Select a FFXV save file",
                Filter = "FFXV save file (gameplay0.save)|gameplay0.save"
            };

            if (saveFileSelect.ShowDialog() == true)
            {
                var snapshotlinkFileSelect = new OpenFileDialog()
                {
                    Title = "Select a snapshotlink file",
                    Filter = "snapshotlink file (snapshotlink.sl)|snapshotlink.sl"
                };

                if (snapshotlinkFileSelect.ShowDialog() == true)
                {
                    var snapshotDirSelect = new VistaFolderBrowserDialog()
                    {
                        Description = "Select a FFXV snapshot directory",
                        UseDescriptionForTitle = true
                    };

                    if (snapshotDirSelect.ShowDialog() == true)
                    {

                    }
                }
            }
        }


        private void DrawOnImgBox(byte[] imgData, int bitmapSrcId, string imgFileName)
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

            if (bitmapSrcId == 0)
            {
                ViewModelInstance.BitmapSrc0 = bitmap;
            }
            else
            {
                ViewModelInstance.BitmapSrc1 = bitmap;
            }

            AppStatusBarTxtBlock.Text = $"Loaded \"{imgFileName}\"";
        }
    }
}