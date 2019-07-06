using System.Windows;
using IPFilter.ViewModels;

namespace IPFilter.Views
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow
    {
        public OptionsWindow()
        {
            InitializeComponent();
        }

        public OptionsViewModel ViewModel
        {
            get { return DataContext as OptionsViewModel; }
            set { DataContext = value; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
