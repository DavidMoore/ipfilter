namespace IPFilter.UI.Apps
{
    using System;
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
            using (var key = Registry.CurrentUser.OpenSubKey(RegistryKeyName, false))
            {
                if (key == null) return ApplicationDetectionResult.NotFound();

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

        public async Task<FilterUpdateResult> UpdateFilterAsync(FilterDownloadResult filter, CancellationToken cancellationToken, IProgress<int> progress)
        {
            var roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
            var destinationPath = Path.Combine(roamingPath, FolderName, "ipfilter.dat");

            using (var destination = File.Open(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                filter.Stream.WriteTo(destination);
            }

            return new FilterUpdateResult { FilterTimestamp = filter.FilterTimestamp };
        }
    }
}