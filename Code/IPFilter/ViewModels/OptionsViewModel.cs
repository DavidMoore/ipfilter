namespace IPFilter.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using Services;

    public class OptionsViewModel : INotifyPropertyChanged
    {
        int? scheduleHours;
        bool isScheduleEnabled;
        bool pendingChanges;
        readonly DestinationPathsProvider pathProvider;
        string errorMessage;
        private string username;
        bool showNotifications;
        bool isUpdateDisabled;
        bool isPreReleaseEnabled;


        public OptionsViewModel()
        {
            pathProvider = new DestinationPathsProvider();
            LoadSettings();
            
            SaveSettingsCommand = new DelegateCommand(SaveSettings, CanSaveSettings);
            ResetSettingsCommand = new DelegateCommand(ResetSettings, CanResetSettings);
        }

        void LoadSettings()
        {
            try
            {
                ErrorMessage = string.Empty;
                Paths = new ObservableCollection<PathSetting>(pathProvider.GetDestinations());
                Paths.CollectionChanged += (sender, args) => PendingChanges = true;

                IsScheduleEnabled = Config.Default.settings.task.isEnabled;
                IsPreReleaseEnabled = Config.Default.settings.update.isPreReleaseEnabled;
                IsUpdateDisabled = Config.Default.settings.update.isDisabled;

                //ScheduleHours = Config.Default.settings.update.isDisabled;
                //Username = Settings.Default.Username;
                //ShowNotifications = Settings.Default.ShowNotifications;
                
                PendingChanges = false;
            }
            catch (Exception e)
            {
                Trace.TraceError("Couldn't load settings: " + e);
                ErrorMessage = "Couldn't load settings: " + e.Message;
            }
        }

        bool CanResetSettings(object o)
        {
            return PendingChanges;
        }

        void ResetSettings(object o)
        {
            Config.Reload();
            LoadSettings();
        }

        bool CanSaveSettings(object o)
        {
            return PendingChanges;
        }

        void SaveSettings(object o)
        {
            ErrorMessage = string.Empty;

            try
            {
                Trace.TraceInformation("Saving settings...");
                Config.Save(Config.Default, Config.DefaultSettings);

                // Ensure the configuration is encrypted
//                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
//                var section = config.GetSection("userSettings/" + typeof(Settings).FullName);
//                if (!section.SectionInformation.IsProtected)
//                {
//                    section.SectionInformation.ProtectSection(nameof(RsaProtectedConfigurationProvider));
//                    section.SectionInformation.ForceSave = true;
//                    config.Save(ConfigurationSaveMode.Full);
//                }

                PendingChanges = false;
                Trace.TraceInformation("Settings saved successfully.");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Couldn't save settings: " + ex.Message;
                return;
            }

            try
            {
                Trace.TraceInformation("Updating schedule settings...");
                Commands.ScheduledTaskCommand.Execute(Config.Default.settings.task.isEnabled);
            }
            catch (UnauthorizedAccessException)
            {
                Trace.TraceError("Can't schedule automated update, as IPFilter isn't being run as an administrator.");
                ErrorMessage = "You must be an administrator and run IPFilter elevated to schedule a task.";
            }
            catch (Exception ex)
            {
                Trace.TraceError("Couldn't schedule automated update: " + ex);
                ErrorMessage = "Couldn't schedule automated update: " + ex.Message;
                return;
            }

            if (o is not Window window) return;
            window.Close();
        }

        public DelegateCommand SaveSettingsCommand { get; private set; }

        public DelegateCommand ResetSettingsCommand { get; private set; }


        public bool PendingChanges
        {
            get => pendingChanges;
            set
            {
                if (value.Equals(pendingChanges)) return;
                pendingChanges = value;
                SaveSettingsCommand?.OnCanExecuteChanged();
                ResetSettingsCommand?.OnCanExecuteChanged();
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PathSetting> Paths { get; set; }

        public bool IsUpdateDisabled
        {
            get => isUpdateDisabled;
            set
            {
                if (value.Equals(isUpdateDisabled)) return;
                isUpdateDisabled = value;
                Config.Default.settings.update.isDisabled = value;
                PendingChanges = true;
                OnPropertyChanged();
            }
        }

        public bool IsScheduleEnabled
        {
            get => isScheduleEnabled;
            set
            {
                if (value.Equals(isScheduleEnabled)) return;
                isScheduleEnabled = value;
                Config.Default.settings.task.isEnabled = value;
                PendingChanges = true;
                OnPropertyChanged();
            }
        }

        public bool ShowNotifications
        {
            get => showNotifications;
            set
            {
                if (value == showNotifications) return;
                showNotifications = value;
                //Settings.Default.ShowNotifications = value;
                PendingChanges = true;
                OnPropertyChanged();
            }
        }
        
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                if (value == errorMessage) return;
                errorMessage = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => username;
            set
            {
                if (value == username) return;
                username = value;
                PendingChanges = true;
                //Settings.Default.Username = value;
                OnPropertyChanged();
            }
        }

        public bool IsPreReleaseEnabled
        {
            get => isPreReleaseEnabled;
            set
            {
                if (value.Equals(isPreReleaseEnabled)) return;
                isPreReleaseEnabled = value;
                Config.Default.settings.update.isPreReleaseEnabled = value;
                PendingChanges = true;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}