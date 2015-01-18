namespace IPFilter.Views
{
    using System.Windows;
    using System.Windows.Navigation;
    using ViewModels;

    /// <summary>
    /// Interaction logic for the main window.
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainWindowViewModel();
        }
        
        public MainWindowViewModel ViewModel
        {
            get { return DataContext as MainWindowViewModel; }
            set { DataContext = value; }
        }
        
        async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.Initialize();
        }

        void LaunchHelp(object sender, RequestNavigateEventArgs e)
        {
            ViewModel.LaunchHelpCommand.Execute(e.Uri);
            e.Handled = true;
        }
    }
}