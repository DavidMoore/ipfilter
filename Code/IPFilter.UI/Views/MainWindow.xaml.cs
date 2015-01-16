namespace IPFilter.UI
{
    using System.Windows;

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
    }
}