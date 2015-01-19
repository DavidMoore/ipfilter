namespace IPFilter.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using Microsoft.Win32.TaskScheduler;
    using Properties;
    using Services;
    using UI.Annotations;

    public class OptionsViewModel : INotifyPropertyChanged
    {
        int? scheduleHours;
        bool isScheduleEnabled;
        bool pendingChanges;
        readonly DestinationPathsProvider pathProvider;
        string errorMessage;

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
                IsScheduleEnabled = Settings.Default.IsScheduleEnabled;
                ScheduleHours = Settings.Default.ScheduleHours;
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
            Settings.Default.Reload();
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
                Settings.Default.Save();
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

                const string taskPath = "IPFilter";

                // Get the service on the local machine
                using (var service = new TaskService())
                {
                    if (!IsScheduleEnabled)
                    {
                        // If we're disabling the scheduling, then delete the task if it exists.
                        Trace.TraceInformation("Schedule is disabled. Removing any existing scheduled task.");
                        service.RootFolder.DeleteTask(taskPath, false);
                        Trace.TraceInformation("Finished updating schedule settings.");
                        return;
                    }

                    using (var existingTask = service.GetTask(taskPath))
                    {
                        Trace.TraceInformation("Setting up the automatic schedule...");
                        using (TaskDefinition task = existingTask != null ? existingTask.Definition : service.NewTask())
                        {
                            task.RegistrationInfo.Description = "Updates the IP Filter for bit torrent clients";

                            task.Triggers.Clear();

                            // Schedule for midnight, then check every x hours (6 by default).
                            var trigger = new TimeTrigger(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0));
                            trigger.Repetition.Interval = TimeSpan.FromHours(ScheduleHours ?? 6);
                            task.Triggers.Add(trigger);

                            task.Actions.Clear();
                            task.Actions.Add(new ExecAction(Process.GetCurrentProcess().MainModule.FileName, "/silent"));

                            task.Settings.RunOnlyIfNetworkAvailable = true;
                            task.Settings.StartWhenAvailable = true;
                            task.Settings.WakeToRun = false;
                            task.Principal.RunLevel = TaskRunLevel.Highest;

                            if (existingTask == null)
                            {
                                service.RootFolder.RegisterTaskDefinition(taskPath, task);
                            }
                            else
                            {
                                existingTask.RegisterChanges();
                            }
                        }
                    }

                    Trace.TraceInformation("Finished scheduling automatic update.");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Couldn't schedule automated update: " + ex);
                ErrorMessage = "Couldn't schedule automated update: " + ex.Message;
            }
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

        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                if (value == errorMessage) return;
                errorMessage = value;
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