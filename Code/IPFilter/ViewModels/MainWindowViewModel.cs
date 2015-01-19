namespace IPFilter.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Deployment.Application;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Apps;
    using ListProviders;
    using Microsoft;
    using Models;
    using Services;
    using UI.Annotations;
    
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        IMirrorProvider selectedMirrorProvider;
        UpdateState state;
        readonly IProgress<ProgressModel> progress;
        readonly ApplicationEnumerator applicationEnumerator;
        CancellationTokenSource cancellationToken;
        List<ApplicationDetectionResult> apps;
        readonly FilterDownloader downloader;
        int progressValue;
        FileMirror selectedFileMirror;
        string statusText;
        readonly StringBuilder log = new StringBuilder(500);
        readonly Dispatcher dispatcher;
        
        public MainWindowViewModel()
        {
            Trace.Listeners.Add(new DelegateTraceListener(null,LogLineAction ));
            Trace.TraceInformation("Initializing...");

            dispatcher = Dispatcher.CurrentDispatcher;

            StatusText = "Ready";
            State = UpdateState.Ready;
            ProgressMax = 100;
            
            Options = new OptionsViewModel();
            Update = new UpdateModel();
            MirrorProviders = new List<IMirrorProvider> {new EmuleSecurity(), new BlocklistMirrorProvider()};
            LaunchHelpCommand = new DelegateCommand(LaunchHelp);
            StartCommand = new DelegateCommand(Start, IsStartEnabled);
            applicationEnumerator = new ApplicationEnumerator();
            downloader = new FilterDownloader();
            
            progress = new Progress<ProgressModel>(ProgressHandler);
            cancellationToken = new CancellationTokenSource();
        }
        
        bool IsStartEnabled(object arg)
        {
            return Update == null || !Update.IsUpdating;
        }

        void LogLineAction(string message)
        {
            log.AppendLine(message);
            OnPropertyChanged("LogData");
        }

        void LogAction(string message)
        {
            log.Append(message);
            OnPropertyChanged("LogData");
        }

        void ProgressHandler(ProgressModel progressModel)
        {
            if (progressModel == null) return;

            ProgressValue = progressModel.Value;
            StatusText = progressModel.Caption;
            State = progressModel.State;
        }

        void LaunchHelp(object uri)
        {
            try
            {
                var url = (Uri) uri;
                Process.Start( url.ToString() );
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Couldn't launch support URL: " + ex);
            }
        }
        
        void Start(object o)
        {
            switch (State)
            {
                case UpdateState.Done:
                case UpdateState.Ready:
                case UpdateState.Cancelled:
                    cancellationToken.Dispose();
                    cancellationToken = new CancellationTokenSource();
                    Task.Factory.StartNew(() => StartAsync(), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                    break;
                    
                case UpdateState.Downloading:
                case UpdateState.Decompressing:
                    cancellationToken.Cancel();
                    break;
            }
        }

        async Task StartAsync()
        {
            try
            {
                var uri = SelectedMirrorProvider.GetUrlForMirror(SelectedFileMirror);

                using (var filter = await downloader.DownloadFilter(new Uri(uri), cancellationToken.Token, progress))
                {
                    cancellationToken.Token.ThrowIfCancellationRequested();

                    if (filter == null)
                    {
                        progress.Report(new ProgressModel(UpdateState.Cancelled, "A filter wasn't downloaded successfully.", 0));
                    }
                    else if (filter.Exception != null)
                    {
                        if (filter.Exception is OperationCanceledException) throw filter.Exception;
                        Trace.TraceError("Problem when downloading: " + filter.Exception);
                        progress.Report(new ProgressModel(UpdateState.Cancelled, "Problem when downloading: " + filter.Exception.Message, 0));
                    }
                    else
                    {
                        foreach (var application in apps)
                        {
                            Trace.TraceInformation("Updating app {0} {1}", application.Description, application.Version);
                            await application.Application.UpdateFilterAsync(filter, cancellationToken.Token, progress);
                        }
                    }

                    if (filter != null && filter.FilterTimestamp != null)
                    {
                        var message = string.Format("Done. List timestamp: {0}", filter.FilterTimestamp.Value.ToLocalTime());
                        Trace.TraceInformation(message);
                        progress.Report(new ProgressModel(UpdateState.Done, message, 100));
                    }
                    else
                    {
                        Trace.TraceInformation("Done.");
                        progress.Report(new ProgressModel(UpdateState.Done, "Done", 100));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Trace.TraceWarning("Update was cancelled.");
                progress.Report(new ProgressModel(UpdateState.Cancelled, "Update was cancelled.", 0));
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem when updating: " + ex);
                progress.Report(new ProgressModel(UpdateState.Cancelled, "Problem when updating: " + ex.Message, 0));
            }
        }

        public UpdateModel Update { get; set; }

        public IList<IMirrorProvider> MirrorProviders { get; set; }
        public OptionsViewModel Options { get; private set; }
        public IEnumerable<ApplicationDetectionResult> Providers { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public string StatusText
        {
            get { return statusText; }
            set
            {
                if (value == statusText) return;
                statusText = value;
                OnPropertyChanged();
            }
        }

        public string ButtonText
        {
            get
            {
                switch (State)
                {
                    case UpdateState.Downloading:
                    case UpdateState.Decompressing:
                        return "Cancel";

                    case UpdateState.Cancelling:
                        return "Cancelling...";
                        
                    default:
                        return "Go";
                }
            }
        }

        public int ProgressValue
        {
            get { return progressValue; }
            set
            {
                if (value == progressValue) return;
                ProgressIsIndeterminate = value < 0;
                progressValue = value;
                OnPropertyChanged();
            }
        }

        public int ProgressMin { get; set; }
        public int ProgressMax { get; set; }

        public IMirrorProvider SelectedMirrorProvider
        {
            get { return selectedMirrorProvider; }
            set
            {
                if (Equals(value, selectedMirrorProvider)) return;
                selectedMirrorProvider = value;
                OnPropertyChanged();
                OnPropertyChanged("Mirrors");
                OnPropertyChanged("SelectedFileMirror");
            }
        }

        public IEnumerable<FileMirror> Mirrors
        {
            get
            {
                var mirrors = SelectedMirrorProvider == null ? Enumerable.Empty<FileMirror>() : SelectedMirrorProvider.GetMirrors();
                var fileMirrors = mirrors as IList<FileMirror> ?? mirrors.ToList();
                SelectedFileMirror = fileMirrors.FirstOrDefault();
                return fileMirrors;
            }
        }

        public FileMirror SelectedFileMirror
        {
            get { return selectedFileMirror; }
            set
            {
                if (Equals(value, selectedFileMirror)) return;
                selectedFileMirror = value;
                OnPropertyChanged();
            }
        }

        public ICommand LaunchHelpCommand { get; private set; }

        public UpdateState State
        {
            get { return state; }
            set
            {
                if (value == state) return;
                state = value;
                OnPropertyChanged();
                OnPropertyChanged("ButtonText");
            }
        }

        public ICommand StartCommand { get; set; }

        public string LogData
        {
            get { return log.ToString(); }
        }

        public bool ProgressIsIndeterminate { get; set; }
        
        public async Task Initialize()
        {
            // Check for updates
            await CheckForUpdates();
            
            SelectedMirrorProvider = MirrorProviders.First();
            
            apps = (await applicationEnumerator.GetInstalledApplications()).ToList();

            if (!apps.Any())
            {
                Trace.TraceWarning("No BitTorrent applications found.");
                return;
            }

            foreach (var result in apps)
            {
                Trace.TraceInformation("Found app {0} version {1} at {2}", result.Description, result.Version, result.InstallLocation);
            }

        }
        
        async Task CheckForUpdates()
        {
            try
            {
                Trace.TraceInformation("Checking for software updates...");
                if (!ApplicationDeployment.IsNetworkDeployed)
                {
                    Trace.TraceInformation("Not a deployed app. Can't check for updates.");
                    return;
                }

                progress.Report(new ProgressModel(UpdateState.Downloading, "Checking for software updates...", -1));
                
                // First, we'll hook up the async handlers before doing the update.

                // Handle required restart of the app after update
                ApplicationDeployment.CurrentDeployment.UpdateCompleted += delegate(object sender, AsyncCompletedEventArgs args)
                {
                    progress.Report(new ProgressModel(UpdateState.Ready, "Update applied. Restart the app.", 100));
                    Update.IsUpdating = false;

                    if (args.Cancelled)
                    {
                        Trace.TraceWarning("Update was cancelled.");
                    }
                    else if (args.Error != null)
                    {
                        Trace.TraceWarning("Unexpected update error: " + args.Error.Message);
                    }
                    else
                    {
                        try
                        {
                            if (MessageBoxHelper.Show(dispatcher, "Restart Application Required", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, "You need to restart the app to apply the update. Restart IPFilter now?") != MessageBoxResult.Yes)
                            {
                                return;
                            }

                            dispatcher.Invoke(DispatcherPriority.Normal, new Action(DeploymentHelper.Restart));
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError("Exception when restarting app after an update: " + ex);
                            Update.ErrorMessage = "Couldn't restart the app to apply update. It will be updated the next time you start the app.";
                        }
                    }
                };

                // Handle progress of the update.
                ApplicationDeployment.CurrentDeployment.UpdateProgressChanged += delegate(object sender, DeploymentProgressChangedEventArgs args)
                {
                    progress.Report(new ProgressModel(UpdateState.Downloading, "Updating application...", args.ProgressPercentage));
                    Update.IsUpdating = true;
                    Update.DownloadPercentage = args.ProgressPercentage;
                };

                // Do the actual update check
                var updateAvailable = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate(false);

                Update.IsUpdateAvailable = updateAvailable.UpdateAvailable;

                if (Update.IsUpdateAvailable)
                {
                    Update.AvailableVersion = updateAvailable.AvailableVersion;
                    Update.IsUpdateRequired = updateAvailable.IsUpdateRequired;
                    Update.MinimumRequiredVersion = updateAvailable.MinimumRequiredVersion;
                    Update.UpdateSizeBytes = updateAvailable.UpdateSizeBytes;
                }

                Trace.TraceInformation("Current version: {0}", Update.CurrentVersion);
                Trace.TraceInformation("Available version: {0}", Update.AvailableVersion == null ? "<no updates>" : Update.AvailableVersion.ToString());

                if (!Update.IsUpdateAvailable ) return;

                if (MessageBoxHelper.Show(dispatcher, "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes,
                    "An update to version {0} is available. Would you like to update now?", Update.AvailableVersion) != MessageBoxResult.Yes)
                {
                    return;
                }

                Trace.TraceInformation("Starting application update...");
                ApplicationDeployment.CurrentDeployment.UpdateAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Application update check failed: " + ex);
            }
            finally
            {
                progress.Report(new ProgressModel(UpdateState.Ready, "Ready", 0));
            }
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}