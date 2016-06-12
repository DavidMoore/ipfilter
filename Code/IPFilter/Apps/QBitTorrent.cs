namespace IPFilter.Apps
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Win32;
    using Models;
    using Services;

    class QBitTorrent : IApplication
    {
        public Task<ApplicationDetectionResult> DetectAsync()
        {
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (var key = baseKey.OpenSubKey(@"SOFTWARE\qBittorrent"))
                {
                    if (key == null) return Task.FromResult(ApplicationDetectionResult.NotFound());

                    var installLocation = (string)key.GetValue("InstallLocation");
                    if (string.IsNullOrWhiteSpace(installLocation)) return Task.FromResult(ApplicationDetectionResult.NotFound());

                    var result = new ApplicationDetectionResult
                    {
                        IsPresent = true,
                        Description = "qBittorrent",
                        InstallLocation = new DirectoryInfo(installLocation),
                        Application = this
                    };

                    if (!result.InstallLocation.Exists) return Task.FromResult(ApplicationDetectionResult.NotFound());

                    var applicationPath = Path.Combine(result.InstallLocation.FullName, "qbittorrent.exe");
                    if (!File.Exists(applicationPath)) return Task.FromResult(ApplicationDetectionResult.NotFound());

                    var version = FileVersionInfo.GetVersionInfo(Path.Combine(result.InstallLocation.FullName, "qbittorrent.exe"));
                    result.Version = version.ProductVersion;

                    return Task.FromResult(result);
                }
            }
        }

        public Task<FilterUpdateResult> UpdateFilterAsync(FilterDownloadResult filter, CancellationToken cancellationToken, IProgress<ProgressModel> progress)
        {
            var filterPath = CacheProvider.FilterPath;

            // Update qBittorrent config
            var qBittorrentIniPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "qBittorrent", "qBittorrent.ini");

            if (File.Exists(qBittorrentIniPath))
            {
                Trace.TraceInformation("Pointing qBittorrent to " + filterPath);
                Trace.TraceInformation("Updating qBittorrent configuration: " + qBittorrentIniPath);

                try
                {
                    WriteIniSetting("Preferences", @"IPFilter\Enabled", "true", qBittorrentIniPath);
                    WriteIniSetting("Preferences", @"IPFilter\File", filterPath.Replace("\\", "/"), qBittorrentIniPath);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Couldn't update qBittorrent configuration: " + ex);
                }
            }
            
            return Task.FromResult(new FilterUpdateResult { FilterTimestamp = filter.FilterTimestamp });
        }

        internal void WriteIniSetting(string section, string name, string value, string filename)
        {
            var result = WritePrivateProfileString(section, name, value, filename);
            if (result != 0) return;
            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
        }

        [DllImport("KERNEL32.DLL", EntryPoint = "WritePrivateProfileString")]
        protected internal static extern int WritePrivateProfileString(string lpAppName,string lpKeyName,string lpValue,string lpFileName);
    }
}