namespace IPFilter.UI
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Annotations;
    using Properties;

    public class OptionsViewModel : INotifyPropertyChanged
    {
        int? scheduleHours;
        bool isScheduleEnabled;
        bool pendingChanges;
        readonly DestinationPathsProvider pathProvider;

        public OptionsViewModel()
        {
            pathProvider = new DestinationPathsProvider();

            LoadSettings();
            
            SaveSettingsCommand = new DelegateCommand(SaveSettings, CanSaveSettings);
            ResetSettingsCommand = new DelegateCommand(ResetSettings, CanResetSettings);
        }

        void LoadSettings()
        {
            Paths = new ObservableCollection<PathSetting>(pathProvider.GetDestinations());
            Paths.CollectionChanged += (sender, args) => PendingChanges = true;
            IsScheduleEnabled = Settings.Default.IsScheduleEnabled;
            ScheduleHours = Settings.Default.ScheduleHours;
            PendingChanges = false;
        }

        bool CanResetSettings(object o)
        {
            return PendingChanges;
        }

        void ResetSettings(object o)
        {
            Settings.Default.Reload();
            LoadSettings();
        }

        bool CanSaveSettings(object o)
        {
            return PendingChanges;
        }

        void SaveSettings(object o)
        {
            Settings.Default.Save();
            PendingChanges = false;
        }

        public DelegateCommand SaveSettingsCommand { get; private set; }

        public DelegateCommand ResetSettingsCommand { get; private set; }


        public bool PendingChanges
        {
            get { return pendingChanges; }
            set
            {
                if (value.Equals(pendingChanges)) return;
                pendingChanges = value;
                if( SaveSettingsCommand != null ) SaveSettingsCommand.OnCanExecuteChanged();
                if (ResetSettingsCommand != null) ResetSettingsCommand.OnCanExecuteChanged();
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PathSetting> Paths { get; set; }

        public bool IsScheduleEnabled
        {
            get { return isScheduleEnabled; }
            set
            {
                if (value.Equals(isScheduleEnabled)) return;
                isScheduleEnabled = value;
                Settings.Default.IsScheduleEnabled = value;
                PendingChanges = true;
                OnPropertyChanged();
            }
        }

        public int? ScheduleHours
        {
            get { return scheduleHours; }
            set
            {
                if (value == scheduleHours) return;
                scheduleHours = value;
                if( value.HasValue ) Settings.Default.ScheduleHours = value.Value;
                PendingChanges = true;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}