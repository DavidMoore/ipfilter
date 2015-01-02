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
}