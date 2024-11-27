﻿using MahApps.Metro.Controls;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace EbonySnapsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private static readonly Dictionary<string, string> SnapshotFilesInDirDict = new Dictionary<string, string>();
        private readonly AppViewModel AppViewModelInstance = new AppViewModel();
        public static byte[] CurrentImgData = new byte[] { };

        public static string CurrentSSName { get; set; }
        private static ListBox SnapshotListBoxComp { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = AppViewModelInstance;
            AppViewModelInstance.IsUIenabled = true;
            AppViewModelInstance.StatusBarTxt = "App launched!";
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
                AppViewModelInstance.BitmapSrc0 = null;
                CurrentImgData = SnapshotHelpers.GetImgDataFromSnapshotFile(snapshotSelect.FileName);
                CurrentSSName = Path.GetFileName(snapshotSelect.FileName);
                DrawOnImgBox(CurrentImgData, 0, CurrentSSName);
            }
        }


        private void SaveImgOption_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog()
            {
                Title = "Save Image file",
                FileName = $"{Path.GetFileNameWithoutExtension(CurrentSSName)}",
                Filter = "All files (*.*)|*.*",
                OverwritePrompt = true,
                RestoreDirectory = true
            };

            if (sfd.ShowDialog() == true && sfd.FileName != null)
            {
                var outImgFile = SnapshotHelpers.SaveImgDataToFile(sfd.FileName, Path.GetDirectoryName(sfd.FileName), CurrentImgData);

                MessageBox.Show($"Saved image file \"{Path.GetFileName(outImgFile)}\"", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                AppViewModelInstance.StatusBarTxt = $"Saved \"{Path.GetFileName(outImgFile)}\"";
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
                        AppViewModelInstance.StatusBarTxt = "Loading snapshot files....";
                        AppViewModelInstance.IsUIenabled = false;
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
                                Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.IsUIenabled = true));
                                Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Loaded snapshot files from directory"));
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    AppViewModelInstance.IsUIenabled = true;
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
                    CurrentImgData = SnapshotHelpers.GetImgDataFromSnapshotFile(imgFile);
                    CurrentSSName = Path.GetFileName(imgFile);
                    DrawOnImgBox(CurrentImgData, 0, CurrentSSName);
                }
            }
        }


        private void SaveSnapshotsInListBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SnapshotFilesInDirDict.Keys.Count != 0)
            {
                var snapshotsSaveDirSelect = new VistaFolderBrowserDialog()
                {
                    Description = "Select a directory to save the image file(s)",
                    UseDescriptionForTitle = true
                };

                if (snapshotsSaveDirSelect.ShowDialog() == true)
                {
                    try
                    {
                        AppViewModelInstance.StatusBarTxt = "Saving snapshot files....";
                        AppViewModelInstance.IsUIenabled = false;

                        System.Threading.Tasks.Task.Run(() =>
                        {
                            try
                            {
                                foreach (var ssFile in SnapshotFilesInDirDict.Values)
                                { 
                                    if (File.Exists(ssFile))
                                    {
                                        _= SnapshotHelpers.SaveImgDataToFile(ssFile, snapshotsSaveDirSelect.SelectedPath, SnapshotHelpers.GetImgDataFromSnapshotFile(ssFile));
                                    }
                                }
                            }
                            finally
                            {
                                Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.IsUIenabled = true));
                                MessageBox.Show("Finished saving all snapshot files from directory", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Saved all snapshot files from directory"));
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        AppViewModelInstance.IsUIenabled = true;
                    }
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
            if (AppViewModelInstance.BitmapSrc1 == null)
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
            if (AppViewModelInstance.BitmapSrc1 == null)
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
                AppViewModelInstance.BitmapSrc0 = bitmap;
            }
            else
            {
                AppViewModelInstance.BitmapSrc1 = bitmap;
            }

            AppViewModelInstance.StatusBarTxt = $"Loaded \"{imgFileName}\"";
        }


        private void SnapViewerImgBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ImgFullScreenForm.ImgData = CurrentImgData;

                var imgFullScreenForm = new ImgFullScreenForm();
                imgFullScreenForm.Show();
            }
        }

        private void SnapToolsImgBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ImgFullScreenForm.ImgData = CurrentImgData;

                var imgFullScreenForm = new ImgFullScreenForm();
                imgFullScreenForm.Show();
            }
        }
    }
}