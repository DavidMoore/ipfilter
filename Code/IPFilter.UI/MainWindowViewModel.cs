namespace IPFilter.UI
{
    public class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            Options = new OptionsViewModel();
        }

        public OptionsViewModel Options { get; private set; }
    }
}