using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using Ionic.Zip;
using IPFilter.UI.Properties;

namespace IPFilter.UI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        const int ZipReadBufferSize = 4096;
        readonly IMirrorProvider mirrorProvider;
        readonly BackgroundWorker worker;
        IEnumerable<FileMirror> mirrors;
        UpdateState state;

        public Window1()
        {
            mirrorProvider = new SourceForgeMirrorProvider();

            worker = new BackgroundWorker
                         {
                             WorkerReportsProgress = true,
                             WorkerSupportsCancellation = true
                         };

            worker.DoWork += DoWork;
            worker.ProgressChanged += ProgressChanged;
            worker.RunWorkerCompleted += RunWorkerCompleted;

            InitializeComponent();
        }

        void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblStatus.Content = e.Cancelled ? "Cancelled" : "Done";
            pbProgress.Value = 100;
            SetState( e.Cancelled ? UpdateState.Cancelled : UpdateState.Done);
        }

        void SetState(UpdateState updateState)
        {
            state = updateState;

            RefreshState();
        }

        void RefreshState()
        {
            switch (state)
            {
                case UpdateState.Cancelled:
                case UpdateState.Ready:
                    cboMirror.IsEnabled = cboMirrorProvider.IsEnabled = true;
                    pbProgress.Value = 0;
                    lblStatus.Content = "Ready";
                    btnGo.Content = "Go";
                    btnGo.IsEnabled = true;
                    break;
                case UpdateState.Downloading:
                    cboMirror.IsEnabled = cboMirrorProvider.IsEnabled = false;
                    btnGo.Content = "Cancel";
                    break;
                case UpdateState.Cancelling:
                    if (worker.CancellationPending) break;
                    btnGo.Content = "Cancelling...";
                    btnGo.IsEnabled = false;
                    CancelDownload();
                    break;
                case UpdateState.Done:
                    btnGo.Content = "Done";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void CancelDownload()
        {
            worker.CancelAsync();
        }

        MessageBoxResult ShowPrompt(string message, string title)
        {
            return ShowMessageBox(title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, message);
        }

        void StartDownload()
        {
            var mirror = ((FileMirror) cboMirror.SelectedItem);
            string urlString = string.Format(CultureInfo.CurrentUICulture, Settings.Default.DownloadUrlFormat, mirror.Id);
            worker.RunWorkerAsync(urlString);
        }


        void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblStatus.Content = e.UserState.ToString();
            pbProgress.Value = e.ProgressPercentage;
        }

        void ZipProgress(object sender, ZipProgressEventArgs args)
        {
            if (args.EventType != ZipProgressEventType.Extracting_EntryBytesWritten) return;
            var percentage = args.BytesTransferred/args.TotalBytesToTransfer * 100;
            worker.ReportProgress( Convert.ToInt32(percentage), "Extracting..." );
        }

        void DoWork(object sender, DoWorkEventArgs e)
        {
            WebRequest request = WebRequest.Create((string) e.Argument);

            worker.ReportProgress(0, string.Format(CultureInfo.CurrentUICulture, "Contacting {0}", request.RequestUri));
            WebResponse response = request.GetResponse();

            const int bufferSize = 1024*64; // Go in 64K chunks

            var buffer = new byte[bufferSize];

            using (var contentStream = new MemoryStream())
            using (Stream stream = response.GetResponseStream())
            {
                worker.ReportProgress(0, "Downloading...");

                int length = Convert.ToInt32(response.ContentLength);
                double lengthMegs = (double) length/1024/1024;

                int bytesRead = stream.Read(buffer, 0, bufferSize);
                int totalRead = 0;

                while (bytesRead > 0)
                {
                    contentStream.Write(buffer, 0, bytesRead);
                    totalRead += bytesRead;

                    double downloadedMegs = (double) totalRead/1024/1024;
                    double percentage = (double) totalRead/length;

                    worker.ReportProgress(Convert.ToInt32(percentage*100),
                                          string.Format(CultureInfo.CurrentUICulture,
                                                        "Downloaded {0:F2} MB of {1:F2} MB", downloadedMegs, lengthMegs));
                    bytesRead = stream.Read(buffer, 0, bufferSize);

                    if (worker.CancellationPending)
                    {
                        // Cancel
                        e.Cancel = true;
                        return;
                    }
                }

                string filterPath = Environment.ExpandEnvironmentVariables(@"%APPDATA%\uTorrent\ipfilter.dat");

                worker.ReportProgress(0, "Decompressing...");

                using (var decompressedStream = new MemoryStream())
                {
                    var zipReadBuffer = new byte[ZipReadBufferSize];

                    MessageBoxResult tryToWrite = MessageBoxResult.Yes;

                    try
                    {
                        contentStream.Seek(0, SeekOrigin.Begin);
                        using( var zipFile = ZipFile.Read(contentStream, ZipProgress) )
                        {
                            if( zipFile.Entries.Count == 0) throw new ZipException("There are no entries in the zip file.");
                            if( zipFile.Entries.Count > 1) throw new ZipException("There is more than one file in the zip file. This application will need to be updated to support this.");
                            
                            var entry = zipFile.Entries.First();

                            entry.Extract(decompressedStream);                            
                        }
                    }
                    catch (Exception ex)
                    {
                        bool isRightToLeft = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;

                        ShowMessageBox("Error Decompressing", // Title / Caption
                                       MessageBoxButton.OK, // Buttons
                                       MessageBoxImage.Error, // Icon
                                       MessageBoxResult.OK, // Default button
                                       "There was a problem decompressing: {0}",
                                       ex.Message
                            ); // RTL culture?

                        return;
                    }

                    if (!File.Exists(filterPath))
                    {
                        var filterDirectory = new DirectoryInfo(Path.GetDirectoryName(filterPath));
                        if (!filterDirectory.Exists) filterDirectory.Create();
                        File.Create(filterPath).Dispose();
                    }

                    worker.ReportProgress(100, "Writing to " + filterPath);

                    while (tryToWrite == MessageBoxResult.Yes)
                    {
                        try
                        {
                            using (
                                FileStream file = File.Open(filterPath, FileMode.Truncate, FileAccess.Write,
                                                            FileShare.None))
                            {
                                decompressedStream.WriteTo(file);
                                file.Flush();
                                file.Close();
                                tryToWrite = MessageBoxResult.No;
                            }
                        }
                        catch (Exception ex)
                        {
                            tryToWrite = ShowMessageBox("Error Writing ipfilter.dat", // Title / Caption
                                                        MessageBoxButton.YesNo, // Buttons
                                                        MessageBoxImage.Error, // Icon
                                                        MessageBoxResult.Yes, // Default button
                                                        "There was a problem writing to {0}:\n\n{1}",
                                                        filterPath, ex.Message
                                ); // RTL culture?
                        }
                    }
                }
            }
        }

        MessageBoxResult ShowMessageBox(string title, MessageBoxButton buttons, MessageBoxImage image,
                                        MessageBoxResult defaultButton, string message, params object[] args)
        {
            string formattedMessage = string.Format(CultureInfo.CurrentCulture, message, args);

            return (MessageBoxResult)Dispatcher.Invoke(new Func<MessageBoxResult>(
                delegate
                    {
                        return MessageBox.Show(this, // Owner
                                   formattedMessage, // Message
                                   title, // Title / Caption
                                   buttons, // Buttons
                                   image, // Icon
                                   defaultButton, // Default button
                            // RTL culture?
                                   CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                                       ?
                                           MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading
                                       : 0);
                    }
                ));
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetStatus("Loading mirrors...");

            btnGo.IsEnabled = false;

            cboMirror.Items.Clear();
            cboMirror.IsEnabled = false;

            cboMirrorProvider.Items.Clear();
            cboMirrorProvider.IsEnabled = false;

            cboMirrorProvider.Items.Add(mirrorProvider);

            cboMirrorProvider.SelectedIndex = 0;

            cboMirrorProvider.IsEnabled = true;

            ThreadPool.QueueUserWorkItem(LoadMirrors);
        }

        void LoadMirrors(object state)
        {
            mirrors = mirrorProvider.GetMirrors();

            Dispatcher.Invoke( new Action( () =>
               {
                   cboMirror.ItemsSource = mirrors;
                   cboMirror.SelectedIndex = mirrors.Count() - 1;
                   cboMirror.IsEnabled = true;
                   SetStatus("Ready");
                   btnGo.IsEnabled = true;
               } ));
    }

        void SetStatus(string message)
        {
            lblStatus.Content = message;
        }

        void btnGo_Click(object sender, RoutedEventArgs e)
        {
            switch (state)
            {
                case UpdateState.Cancelled:
                case UpdateState.Ready:
                    state = UpdateState.Downloading;
                    StartDownload();
                    break;
                case UpdateState.Downloading:
                    state = UpdateState.Cancelling;
                    break;
                case UpdateState.Done:
                    SetState(UpdateState.Ready);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            cboMirror.IsEnabled = cboMirrorProvider.IsEnabled = false;
            RefreshState();
        }

        void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}