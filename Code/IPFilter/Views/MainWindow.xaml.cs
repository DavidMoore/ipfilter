namespace IPFilter.Views
{
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Navigation;
    using Native;
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
            this.Activated += (sender, args) =>
            {
                var helper = new WindowInteropHelper(Application.Current.MainWindow);
                Win32Api.BringToFront(helper.Handle);
            };
        }
        
        public MainWindowViewModel ViewModel
        {
            get { return DataContext as MainWindowViewModel; }
            set { DataContext = value; }
        }
        
        async void Window_Loaded(object sender, RoutedEventArgs e)
        {
//            var helper = new WindowInteropHelper(Application.Current.MainWindow);
//            Win32Api.BringToFront(helper.Handle);
            await ViewModel.Initialize();
        }

        void LaunchHelp(object sender, RequestNavigateEventArgs e)
        {
            ViewModel.LaunchHelpCommand.Execute(e.Uri);
            e.Handled = true;
        }
    }
}