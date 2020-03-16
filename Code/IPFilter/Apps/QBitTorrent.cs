using IPFilter.Native;

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
    using Formats;

    /// <summary>
    /// qBittorrent support
    /// </summary>
    class QBitTorrent : IApplication
    {
        const string FolderName = "qBittorrent";

        public Task<ApplicationDetectionResult> DetectAsync()
        {
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (var key = baseKey.OpenSubKey(@"SOFTWARE\qBittorrent"))
                {
                    if (key == null) return Task.FromResult(ApplicationDetectionResult.NotFound());

                    var installLocation = (string)key.GetValue("InstallLocation");
                    if (string.IsNullOrWhiteSpace(installLocation)) return Task.FromResult(ApplicationDetectionResult.NotFound());

                    var result = ApplicationDetectionResult.Create(this, "qBittorrent", installLocation);
                    
                    if (result.InstallLocation == null || !result.InstallLocation.Exists) return Task.FromResult(ApplicationDetectionResult.NotFound());

                    var applicationPath = Path.Combine(result.InstallLocation.FullName, "qbittorrent.exe");
                    if (!File.Exists(applicationPath)) return Task.FromResult(ApplicationDetectionResult.NotFound());

                    var version = FileVersionInfo.GetVersionInfo(Path.Combine(result.InstallLocation.FullName, "qbittorrent.exe"));
                    result.Version = version.ProductVersion;

                    return Task.FromResult(result);
                }
            }
        }

        public async Task<FilterUpdateResult> UpdateFilterAsync(FilterDownloadResult filter, CancellationToken cancellationToken, IProgress<ProgressModel> progress)
        {
            var localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
            var destinationPath = Path.Combine(localPath, FolderName, "filter", "ipfilter.dat");
            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (destinationDirectory != null && !Directory.Exists(destinationDirectory)) Directory.CreateDirectory(destinationDirectory);

            Trace.TraceInformation("Writing filter to " + destinationPath);
            using (var destination = File.Open(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new P2pWriter(destination))
            {
                await writer.Write(filter.Entries, progress);
            }

            // Update qBittorrent config
            var qBittorrentIniPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FolderName, "qBittorrent.ini");

            if (File.Exists(qBittorrentIniPath))
            {
                Trace.TraceInformation("Pointing qBittorrent to " + destinationPath);
                Trace.TraceInformation("Updating qBittorrent configuration: " + qBittorrentIniPath);

                try
                {
                    WriteIniSetting("Preferences", @"IPFilter\Enabled", "true", qBittorrentIniPath);
                    WriteIniSetting("Preferences", @"IPFilter\File", destinationPath.Replace("\\", "/"), qBittorrentIniPath);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Couldn't update qBittorrent configuration: " + ex);
                }
            }
            
            return new FilterUpdateResult { FilterTimestamp = filter.FilterTimestamp };
        }

        static void WriteIniSetting(string section, string name, string value, string filename)
        {
            var result = WritePrivateProfileString(section, name, value, filename);
            if (result != 0) return;
            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
        }

        [DllImport("KERNEL32.DLL", EntryPoint = "WritePrivateProfileString")]
        static extern int WritePrivateProfileString(string lpAppName,string lpKeyName,string lpValue,string lpFileName);
    }
}