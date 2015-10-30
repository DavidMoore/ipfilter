namespace IPFilter.Apps
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Win32;
    using Models;

    class BitTorrentApplication : IApplication
    {
        protected virtual string RegistryKeyName { get { return @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + FolderName; } }

        protected virtual string DefaultDisplayName { get { return "BitTorrent"; } }

        protected virtual string FolderName { get { return "BitTorrent"; } }

        public async Task<ApplicationDetectionResult> DetectAsync()
        {
            // Look in HKCU first (current user install), then fall back to HKLM (all users install).
            var key = Registry.CurrentUser.OpenSubKey(RegistryKeyName, false);
            if (key == null)
            {
                key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(RegistryKeyName, false);
            }

            if( key == null ) return ApplicationDetectionResult.NotFound();

            using (key)
            {
                var installLocation = (string)key.GetValue("InstallLocation");
                if (installLocation == null) return ApplicationDetectionResult.NotFound();

                var displayName = (string)key.GetValue("DisplayName") ?? DefaultDisplayName;
                var version = (string)key.GetValue("DisplayVersion") ?? "Unknown";

                var result = new ApplicationDetectionResult
                {
                    IsPresent = true,
                    Description = displayName,
                    InstallLocation = new DirectoryInfo(installLocation),
                    Version = version,
                    Application = this
                };

                if (!result.InstallLocation.Exists) result.IsPresent = false;

                return result;
            }
        }

        public async Task<FilterUpdateResult> UpdateFilterAsync(FilterDownloadResult filter, CancellationToken cancellationToken, IProgress<ProgressModel> progress)
        {
            var roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
            var destinationPath = Path.Combine(roamingPath, FolderName, "ipfilter.dat");

            Trace.TraceInformation("Writing filter to " + destinationPath);
            using (var destination = File.Open(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                filter.Stream.WriteTo(destination);
            }

            return new FilterUpdateResult { FilterTimestamp = filter.FilterTimestamp };

            // TODO: Check if IP Filter is enabled in µTorrent
//            string settingsPath = Environment.ExpandEnvironmentVariables(@"%APPDATA%\uTorrent\settings.dat");
//            var settings = File.ReadAllText(settingsPath);
//            if (settings.Contains("15:ipfilter.enablei0e"))
//            {
//                MessageBox.Show("You haven't enabled IP Filtering in µTorrent! Go to http://ipfilter.codeplex.com/ for help.", "IP filtering not enabled", MessageBoxButton.OK);
//            }
        }
    }
}