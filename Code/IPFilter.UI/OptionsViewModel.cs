namespace IPFilter.UI
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using Properties;

    public class OptionsViewModel
    {
        DestinationPathsProvider pathProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public OptionsViewModel()
        {
            pathProvider = new DestinationPathsProvider();

            Paths = new ObservableCollection<PathSetting>( pathProvider.GetDestinations() );
        }

        public ObservableCollection<PathSetting> Paths { get; set; } 
    }

    public class PathSetting
    {
        public PathSetting(string name, string path, bool isDefault)
        {
            Name = name;
            Path = path;
            IsDefault = isDefault;
        }

        public bool IsDefault { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}