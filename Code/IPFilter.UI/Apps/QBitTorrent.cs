namespace IPFilter.UI.Apps
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Win32;
    using Models;

    class QBitTorrent : IApplication
    {
        public async Task<ApplicationDetectionResult> DetectAsync()
        {
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (var key = baseKey.OpenSubKey(@"SOFTWARE\qBittorrent"))
                {
                    if (key == null) return ApplicationDetectionResult.NotFound();

                    var installLocation = (string)key.GetValue("InstallLocation");
                    if (string.IsNullOrWhiteSpace(installLocation)) return ApplicationDetectionResult.NotFound();

                    var result = new ApplicationDetectionResult
                    {
                        IsPresent = true,
                        Description = "qBittorrent",
                        InstallLocation = new DirectoryInfo(installLocation),
                        Application = this
                    };

                    if (!result.InstallLocation.Exists) return ApplicationDetectionResult.NotFound();

                    var applicationPath = Path.Combine(result.InstallLocation.FullName, "qbittorrent.exe");
                    if (!File.Exists(applicationPath)) return ApplicationDetectionResult.NotFound();

                    var version = FileVersionInfo.GetVersionInfo(Path.Combine(result.InstallLocation.FullName, "qbittorrent.exe"));
                    result.Version = version.ProductVersion;

                    return result;
                }
            }
        }

        public async Task<FilterUpdateResult> UpdateFilterAsync(FilterDownloadResult filter, CancellationToken cancellationToken, IProgress<int> progress)
        {
            var filterPath = CacheProvider.FilterPath;

            // Update qBittorrent config
            var qBittorrentIniPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "qBittorrent", "qBittorrent.ini");

            if (File.Exists(qBittorrentIniPath))
            {
                WritePrivateProfileString("Preferences", @"IPFilter\Enabled", "true", qBittorrentIniPath);
                WritePrivateProfileString("Preferences", @"IPFilter\File", filterPath.Replace("\\", "\\\\"), qBittorrentIniPath);
            }
            
            return new FilterUpdateResult { FilterTimestamp = filter.FilterTimestamp };
        }

        [DllImport("KERNEL32.DLL", EntryPoint = "WritePrivateProfileString")]
        protected internal static extern int WritePrivateProfileString(string lpAppName,string lpKeyName,string lpValue,string lpFileName);
    }
}