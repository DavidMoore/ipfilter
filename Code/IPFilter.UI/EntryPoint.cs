using System;
using System.Deployment.Application;
using System.IO;

namespace IPFilter.UI
{
    using System.Diagnostics;
    using Properties;

    static class EntryPoint
    {
        [STAThread]
        internal static void Main(string[] args)
        {
            UpgradeSettings();
            
            // TODO: Command line arguments / run silently

            // Create the view model
            var viewModel = new MainWindowViewModel();
            
            var window = new MainWindow(viewModel);
            var app = new App();
            app.Run(window);
        }

        static void UpgradeSettings()
        {
            try
            {
                // Upgrade / migrate custom settings if necessary
                Settings.Default.Upgrade();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Couldn't upgrade settings: " + ex);
            }
        }


        static void AddToStartup()
        {
            const string link =
                "http://ipfilterupdate.sourceforge.net/install/IPFilter.UI.application#IPFilter.UI.application, Culture=neutral, PublicKeyToken=0000000000000000, processorArchitecture=msil";
        }

        public static void AddShortcutToStartupGroup(string publisherName, string productName)
        {
            if (ApplicationDeployment.IsNetworkDeployed && ApplicationDeployment.CurrentDeployment.IsFirstRun)
            {
                string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

                startupPath = Path.Combine(startupPath, productName) + ".appref-ms";
                if (!File.Exists(startupPath))
                {
                    string allProgramsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                    string shortcutPath = Path.Combine(allProgramsPath, publisherName);
                    shortcutPath = Path.Combine(shortcutPath, productName) + ".appref-ms";
                    File.Copy(shortcutPath, startupPath);
                }
            }
        }
    }

    public class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            Options = new OptionsViewModel();
        }

        public OptionsViewModel Options { get; private set; }
    }
}
