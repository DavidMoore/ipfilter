namespace IPFilter.Views
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Interop;
    using System.Windows.Navigation;
    using System.Windows.Resources;
    using Native;
    using ViewModels;
    using Application = System.Windows.Application;

    /// <summary>
    /// Interaction logic for the main window.
    /// </summary>
    public partial class MainWindow
    {
        readonly NotifyIcon notifyIcon;
        readonly WindowInteropHelper helper;
        readonly ContextMenu contextMenu;

        public MainWindow()
        {
            InitializeComponent();

            helper = new WindowInteropHelper(this);
            
            ViewModel = new MainWindowViewModel();
            ViewModel.ShowNotification = (title, message, icon) => notifyIcon.ShowBalloonTip(3000, title, message, icon);

            Closing += OnClosing;

            Activated += (sender, args) =>
            {
                helper.EnsureHandle();
                Win32Api.BringToFront(helper.Handle);
            };

            notifyIcon = new NotifyIcon();

            StreamResourceInfo resourceStream = Application.GetResourceStream(new Uri("pack://application:,,,/App.ico"));
            if (resourceStream != null)
            {
                using (Stream iconStream = resourceStream.Stream)
                {
                    notifyIcon.Icon = new System.Drawing.Icon(iconStream);
                }
            }
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += OnNotifyIconClick;
            notifyIcon.Text = ViewModel.Update.ProductAndVersion;

            //notifyIcon.Click += OnNotifyIconClick;

            contextMenu = new ContextMenu();
            var exitMenuItem = new MenuItem("E&xit", (sender, args) => Application.Current.Shutdown());
            contextMenu.MenuItems.Add(exitMenuItem);

            notifyIcon.ContextMenu = contextMenu;

        }

        void OnNotifyIconClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) return;

            Show();
            WindowState = WindowState.Normal;
            Win32Api.BringToFront(helper.Handle);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }

            base.OnStateChanged(e);
        }

        void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            cancelEventArgs.Cancel = true;
            WindowState = WindowState.Minimized;
        }

        /// <summary>Raises the <see cref="E:System.Windows.Window.Closed" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            notifyIcon.Dispose();
            ViewModel.Shutdown();
            base.OnClosed(e);
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