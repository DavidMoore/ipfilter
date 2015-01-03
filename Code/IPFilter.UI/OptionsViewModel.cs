namespace IPFilter.UI
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using Annotations;
    using Properties;

    public class OptionsViewModel : INotifyPropertyChanged
    {
        int? scheduleHours;
        bool isScheduleEnabled;
        bool pendingChanges;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public OptionsViewModel()
        {
            var pathProvider = new DestinationPathsProvider();

            Paths = new ObservableCollection<PathSetting>( pathProvider.GetDestinations() );
            
            IsScheduleEnabled = Settings.Default.IsScheduleEnabled;
            ScheduleHours = Settings.Default.ScheduleHours;
            PendingChanges = false;

            SaveSettings = new DelegateCommand(Action, OnCanExecute);
        }

        bool OnCanExecute(object o)
        {
            return PendingChanges;
        }

        void Action(object o)
        {
            Settings.Default.Save();
            PendingChanges = false;
        }

        public DelegateCommand SaveSettings { get; private set; }


        public bool PendingChanges
        {
            get { return pendingChanges; }
            set
            {
                if (value.Equals(pendingChanges)) return;
                pendingChanges = value;
                if( SaveSettings != null ) SaveSettings.OnCanExecuteChanged();
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