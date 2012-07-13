using System;
using System.Deployment.Application;
using System.IO;

namespace IPFilter.UI
{
    static class EntryPoint
    {
        [STAThread]
        internal static void Main(string[] args)
        {
            var app = new App();

            var window = new MainWindow();

            app.Run(window);
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
}
