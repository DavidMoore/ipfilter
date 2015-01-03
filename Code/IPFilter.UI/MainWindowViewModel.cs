namespace IPFilter.UI
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Annotations;
    using ListProviders;

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            Options = new OptionsViewModel();

            Mirrors = new List<IMirrorProvider> {new EmuleSecurity(), new BlocklistMirrorProvider()};
        }

        public IList<IMirrorProvider> Mirrors { get; set; }
        public OptionsViewModel Options { get; private set; }
        public IEnumerable<ApplicationDetectionResult> Providers { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public async Task Initialize()
        {
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}