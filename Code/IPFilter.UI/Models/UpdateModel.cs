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

    public class UpdateModel : INotifyPropertyChanged
    {
        string product;
        string currentVersion;
        bool isUpdateAvailable;
        Version availableVersion;
        bool isUpdateRequired;
        Version minimumRequiredVersion;
        long updateSizeBytes;
        Visibility updateAvailableVisibility;
        int downloadPercentage;
        string productAndVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public UpdateModel()
        {
            CurrentVersion = GetAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            Product = GetAttribute<AssemblyProductAttribute>().Product;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                CurrentVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
        }

        public string Product
        {
            get { return product; }
            set
            {
                if (value == product) return;
                product = value;
                OnPropertyChanged();
                OnPropertyChanged("ProductAndVersion");
            }
        }

        public string CurrentVersion
        {
            get { return currentVersion; }
            set
            {
                if (value == currentVersion) return;
                currentVersion = value;
                OnPropertyChanged();
                OnPropertyChanged("ProductAndVersion");
            }
        }

        public string ProductAndVersion
        {
            get { return Product + " v" + CurrentVersion; }
        }

        public bool IsUpdateAvailable
        {
            get { return isUpdateAvailable; }
            set
            {
                if (value.Equals(isUpdateAvailable)) return;
                isUpdateAvailable = value;
                OnPropertyChanged();
                OnPropertyChanged("UpdateAvailableVisibility");
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
            set
            {
                if (value == updateAvailableVisibility) return;
                updateAvailableVisibility = value;
                OnPropertyChanged();
            }
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

        public bool IsUpdating { get; set; }
        public string ErrorMessage { get; set; }

        static T GetAttribute<T>() where T : Attribute
        {
            return Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(T), true).Cast<T>().Single();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}