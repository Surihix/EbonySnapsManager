using EbonySnapsManager.Helpers;
using EbonySnapsManager.LargeProcesses;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        internal static readonly Dictionary<string, string> SnapshotFilesInDirDict = new Dictionary<string, string>();
        private readonly AppViewModel AppViewModelInstance = new AppViewModel();

        private static byte[] CurrentSnapshotData = new byte[] { };
        private static byte[] CurrentImgData = new byte[] { };

        public static string CurrentSSName { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = AppViewModelInstance;
            AppViewModelInstance.IsUIenabled = true;
            AppViewModelInstance.StatusBarTxt = "Welcome to Ebony Snaps Manager!";
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
                try
                {
                    AppViewModelInstance.BitmapSrc0 = null;
                    CurrentSnapshotData = SnapshotHelpers.GetImgDataFromSnapshotFile(snapshotSelect.FileName);
                    CurrentSSName = Path.GetFileName(snapshotSelect.FileName);

                    DrawOnImgBox(CurrentSnapshotData, 0, CurrentSSName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    AppViewModelInstance.StatusBarTxt = "Failed to load snapshot file";
                }
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
                try
                {
                    var outImgFile = SnapshotHelpers.SaveImgDataToFile(sfd.FileName, Path.GetDirectoryName(sfd.FileName), CurrentSnapshotData);

                    MessageBox.Show($"Saved image file \"{Path.GetFileName(outImgFile)}\"", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    AppViewModelInstance.StatusBarTxt = $"Saved \"{Path.GetFileName(outImgFile)}\"";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    AppViewModelInstance.StatusBarTxt = "Failed to save image file";
                }
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
                var snapshotDir = Directory.GetFiles(snapshotDirSelect.SelectedPath, "*.ss", SearchOption.TopDirectoryOnly);

                if (snapshotDir.Length == 0)
                {
                    MessageBox.Show("Unable to find valid FFXV snapshot files in the selected folder", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    AppViewModelInstance.StatusBarTxt = "Loading snapshot files....";
                    AppViewModelInstance.IsUIenabled = false;

                    Task.Run(() =>
                    {
                        try
                        {
                            Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.ListboxItemsSource.Clear()));
                            SnapshotFilesInDirDict.Clear();

                            foreach (var ssFile in snapshotDir)
                            {
                                var ssFileName = Path.GetFileName(ssFile);
                                SnapshotFilesInDirDict.Add(ssFileName, ssFile);
                                Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.ListboxItemsSource.Add(ssFileName)));
                            }

                            Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Loaded snapshot files from directory"));
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(new Action(() => MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)));
                            Dispatcher.Invoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Failed to load snapshot files"));
                        }
                        finally
                        {
                            Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.IsUIenabled = true));
                        }
                    });
                }
            }
        }


        private void SnapshotListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SnapshotListbox.SelectedItem != null)
            {
                try
                {
                    if (File.Exists(SnapshotFilesInDirDict[(string)SnapshotListbox.SelectedItem]))
                    {
                        var imgFile = SnapshotFilesInDirDict[(string)SnapshotListbox.SelectedItem];
                        CurrentSnapshotData = SnapshotHelpers.GetImgDataFromSnapshotFile(imgFile);
                        CurrentSSName = Path.GetFileName(imgFile);
                        DrawOnImgBox(CurrentSnapshotData, 0, CurrentSSName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    AppViewModelInstance.StatusBarTxt = "Failed to load selected snapshot file";
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
                    AppViewModelInstance.IsUIenabled = false;

                    Task.Run(() =>
                    {
                        try
                        {
                            Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Saving snapshot files...."));

                            foreach (var ssFile in SnapshotFilesInDirDict.Values)
                            {
                                if (File.Exists(ssFile))
                                {
                                    _ = SnapshotHelpers.SaveImgDataToFile(ssFile, snapshotsSaveDirSelect.SelectedPath, SnapshotHelpers.GetImgDataFromSnapshotFile(ssFile));
                                }
                            }

                            Dispatcher.Invoke(new Action(() => MessageBox.Show("Finished saving all snapshot files from directory", "Success", MessageBoxButton.OK, MessageBoxImage.Information)));
                            Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Saved all snapshot files from directory"));
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(new Action(() => MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)));
                            Dispatcher.Invoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Failed to save snapshot files"));
                        }
                        finally
                        {
                            Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.IsUIenabled = true));
                        }
                    });
                }
            }
            else
            {
                MessageBox.Show("Unable to find snapshot files on the SnapsList. Please load a valid snapshot directory before using this option", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                try
                {
                    var imgFile = imgSelect.FileName;
                    CurrentImgData = File.ReadAllBytes(imgFile);
                    DrawOnImgBox(CurrentImgData, 1, Path.GetFileName(imgFile));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    AppViewModelInstance.StatusBarTxt = "Failed to load image file";
                }
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
                var snapshotlinkFileSelect = new OpenFileDialog()
                {
                    Title = "Select a snapshotlink file",
                    Filter = "snapshotlink.sl|snapshotlink.sl"
                };

                if (snapshotlinkFileSelect.ShowDialog() == true)
                {
                    var saveFileSelect = new OpenFileDialog()
                    {
                        Title = "Select a FFXV save file",
                        Filter = "gameplay0.save|gameplay0.save"
                    };

                    if (saveFileSelect.ShowDialog() == true)
                    {
                        AppViewModelInstance.IsUIenabled = false;

                        Task.Run(() =>
                        {
                            try
                            {
                                var snapId = uint.MinValue;

                                Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Updating snapshotlink file...."));
                                SnapshotProcesses.AddSnapsInLink(snapshotlinkFileSelect.FileName, ref snapId, 1);

                                Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Updating save file...."));
                                SavedataProcesses.AddSnapsInSave(saveFileSelect.FileName, snapId, 1);

                                Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Creating new snapshot file...."));
                                var newSnapshotFile = Path.Combine(Path.GetDirectoryName(snapshotlinkFileSelect.FileName), Convert.ToString(snapId).PadLeft(8, '0')) + ".ss";

                                if (File.Exists(newSnapshotFile))
                                {
                                    File.Delete(newSnapshotFile);
                                }

                                SnapshotHelpers.CreateSnapshotFile(newSnapshotFile, CurrentImgData);

                                Dispatcher.Invoke(new Action(() => MessageBox.Show("Finished adding new snap", "Success", MessageBoxButton.OK, MessageBoxImage.Information)));
                                Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Added new snap"));
                            }
                            catch (Exception ex)
                            {
                                Dispatcher.Invoke(new Action(() => MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)));
                                Dispatcher.Invoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Failed to add new snap"));
                            }
                            finally
                            {
                                Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.IsUIenabled = true));
                            }
                        });
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
                    Title = "Select a FFXV Snapshot file to replace",
                    Filter = "Snapshot file (*.ss)|*.ss"
                };

                if (snapshotFileSelect.ShowDialog() == true)
                {
                    try
                    {
                        SnapshotHelpers.CreateSnapshotFile(snapshotFileSelect.FileName, CurrentImgData);

                        MessageBox.Show($"Replaced image data in \"{Path.GetFileName(snapshotFileSelect.FileName)}\"", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        AppViewModelInstance.StatusBarTxt = $"Replaced data in \"{Path.GetFileName(snapshotFileSelect.FileName)}\"";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        AppViewModelInstance.StatusBarTxt = "Failed to replace snapshot file's data";
                    }
                }
            }
        }


        private void AddMultipleNewSnapsBtn_Click(object sender, RoutedEventArgs e)
        {
            var imgDir = new VistaFolderBrowserDialog()
            {
                Description = "Select a directory containing image file(s)",
                UseDescriptionForTitle = true
            };

            if (imgDir.ShowDialog() == true)
            {
                var imgFilesList = new List<string>();

                imgFilesList.AddRange(Directory.GetFiles(imgDir.SelectedPath, "*.jpg", SearchOption.TopDirectoryOnly));
                imgFilesList.AddRange(Directory.GetFiles(imgDir.SelectedPath, "*.png", SearchOption.TopDirectoryOnly));

                var imgDirFiles = imgFilesList.ToArray();

                if (imgDirFiles.Length == 0)
                {
                    MessageBox.Show("Unable to find valid image file(s) in the selected folder", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    var snapshotlinkFileSelect = new OpenFileDialog()
                    {
                        Title = "Select a snapshotlink file",
                        Filter = "snapshotlink.sl|snapshotlink.sl"
                    };

                    if (snapshotlinkFileSelect.ShowDialog() == true)
                    {
                        var saveFileSelect = new OpenFileDialog()
                        {
                            Title = "Select a FFXV save file",
                            Filter = "gameplay0.save|gameplay0.save"
                        };

                        if (saveFileSelect.ShowDialog() == true)
                        {
                            AppViewModelInstance.IsUIenabled = false;

                            Task.Run(() =>
                            {
                                try
                                {
                                    var snapId = uint.MinValue;

                                    Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Updating snapshotlink file...."));
                                    SnapshotProcesses.AddSnapsInLink(snapshotlinkFileSelect.FileName, ref snapId, imgDirFiles.Length);

                                    Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Updating save file...."));
                                    SavedataProcesses.AddSnapsInSave(saveFileSelect.FileName, snapId, imgDirFiles.Length);

                                    Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Creating new snapshot file(s)...."));

                                    foreach (var imgFile in imgDirFiles)
                                    {
                                        var newSnapshotFile = Path.Combine(Path.GetDirectoryName(snapshotlinkFileSelect.FileName), Convert.ToString(snapId).PadLeft(8, '0')) + ".ss";

                                        if (File.Exists(newSnapshotFile))
                                        {
                                            File.Delete(newSnapshotFile);
                                        }

                                        SnapshotHelpers.CreateSnapshotFile(newSnapshotFile, File.ReadAllBytes(imgFile));
                                        snapId++;
                                    }

                                    Dispatcher.Invoke(new Action(() => MessageBox.Show("Finished adding new snap(s)", "Success", MessageBoxButton.OK, MessageBoxImage.Information)));
                                    Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Added new snap(s)"));
                                }
                                catch (Exception ex)
                                {
                                    Dispatcher.Invoke(new Action(() => MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)));
                                    Dispatcher.Invoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Failed to add new snap(s)"));
                                }
                                finally
                                {
                                    Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.IsUIenabled = true));
                                }
                            });
                        }
                    }
                }
            }
        }


        private void RemoveBlankSnapsBtn_Click(object sender, RoutedEventArgs e)
        {
            var saveFileSelect = new OpenFileDialog()
            {
                Title = "Select a FFXV save file",
                Filter = "gameplay0.save|gameplay0.save"
            };

            if (saveFileSelect.ShowDialog() == true)
            {
                var snapshotlinkFileSelect = new OpenFileDialog()
                {
                    Title = "Select a snapshotlink file",
                    Filter = "snapshotlink.sl|snapshotlink.sl"
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
                        AppViewModelInstance.IsUIenabled = false;

                        Task.Run(() =>
                        {
                            try
                            {
                                Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Updating save file...."));
                                SavedataProcesses.RemoveBlankSnapsInSave(saveFileSelect.FileName, snapshotDirSelect.SelectedPath);

                                Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Updating snapshotlink file...."));
                                SnapshotProcesses.RemoveBlankSnapsInlink(snapshotlinkFileSelect.FileName, snapshotDirSelect.SelectedPath);

                                Dispatcher.Invoke(new Action(() => MessageBox.Show("Finished removing blank snaps", "Success", MessageBoxButton.OK, MessageBoxImage.Information)));
                                Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Removed blank snaps"));
                            }
                            catch (Exception ex)
                            {
                                Dispatcher.Invoke(new Action(() => MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)));
                                Dispatcher.Invoke(new Action(() => AppViewModelInstance.StatusBarTxt = "Failed to remove blank snaps"));
                            }
                            finally
                            {
                                Dispatcher.BeginInvoke(new Action(() => AppViewModelInstance.IsUIenabled = true));
                            }
                        });
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
                ImgFullScreenForm.ImgData = CurrentSnapshotData;
                ImgFullScreenForm.IsSnapshotFile = true;
                ImgFullScreenForm.CurrentSSName = CurrentSSName;

                var imgFullScreenForm = new ImgFullScreenForm();
                imgFullScreenForm.ShowDialog();
            }
        }


        private void SnapToolsImgBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ImgFullScreenForm.ImgData = CurrentImgData;
                ImgFullScreenForm.IsSnapshotFile = false;

                var imgFullScreenForm = new ImgFullScreenForm();
                imgFullScreenForm.ShowDialog();
            }
        }
    }
}