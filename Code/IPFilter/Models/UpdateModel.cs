namespace IPFilter.Models
{
    using System;
    using System.ComponentModel;
    using System.Deployment.Application;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using UI.Annotations;
    using ViewModels;

    public class UpdateModel : INotifyPropertyChanged
    {
        string product;
        Version currentVersion;
        bool isUpdateAvailable;
        Version availableVersion;
        bool isUpdateRequired;
        Version minimumRequiredVersion;
        long updateSizeBytes;
        int downloadPercentage;
        string errorMessage;
        bool isUpdating;
        
        public UpdateModel()
        {
            CurrentVersion = new Version(GetAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            Product = GetAttribute<AssemblyProductAttribute>().Product;
            UpdateCommand = new DelegateCommand(DoUpdate, CanDoUpdate);

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                CurrentVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion;
            }
        }

        bool CanDoUpdate(object o)
        {
            return AvailableVersion != null && AvailableVersion > CurrentVersion;
        }

        void DoUpdate(object o)
        {
            
        }

        public string Product
        {
            get { return product; }
            set
            {
                if (value == product) return;
                product = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProductAndVersion));
            }
        }

        public Version CurrentVersion
        {
            get { return currentVersion; }
            set
            {
                if (value == currentVersion) return;
                currentVersion = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProductAndVersion));
            }
        }

        public string ProductAndVersion => $"{Product} v{CurrentVersion}";

        public bool IsUpdateAvailable
        {
            get { return isUpdateAvailable; }
            set
            {
                if (value.Equals(isUpdateAvailable)) return;
                isUpdateAvailable = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UpdateAvailableVisibility));
                UpdateCommand.OnCanExecuteChanged();
            }
        }

        public Version AvailableVersion
        {
            get { return availableVersion; }
            set
            {
                if (Equals(value, availableVersion)) return;
                availableVersion = value;
                OnPropertyChanged();
                UpdateCommand.OnCanExecuteChanged();
            }
        }

        public bool IsUpdateRequired
        {
            get { return isUpdateRequired; }
            set
            {
                if (value.Equals(isUpdateRequired)) return;
                isUpdateRequired = value;
                OnPropertyChanged();
            }
        }

        public Version MinimumRequiredVersion
        {
            get { return minimumRequiredVersion; }
            set
            {
                if (Equals(value, minimumRequiredVersion)) return;
                minimumRequiredVersion = value;
                OnPropertyChanged();
            }
        }

        public long UpdateSizeBytes
        {
            get { return updateSizeBytes; }
            set
            {
                if (value == updateSizeBytes) return;
                updateSizeBytes = value;
                OnPropertyChanged();
            }
        }

        public Visibility UpdateAvailableVisibility
        {
            get { return IsUpdateAvailable ? Visibility.Visible : Visibility.Hidden; }
        }

        public int DownloadPercentage
        {
            get { return downloadPercentage; }
            set
            {
                if (value == downloadPercentage) return;
                downloadPercentage = value;
                OnPropertyChanged();
            }
        }

        public bool IsUpdating
        {
            get { return isUpdating; }
            set
            {
                if (value.Equals(isUpdating)) return;
                isUpdating = value;
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

        public DelegateCommand UpdateCommand { get; }

        static T GetAttribute<T>() where T : Attribute
        {
            return Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(T), true).Cast<T>().Single();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}