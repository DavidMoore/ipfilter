using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IPFilter.Formats;
using IPFilter.Models;
using IPFilter.Native;
using Microsoft.Win32;

namespace IPFilter.Apps
{
    class DelugeApplication : IApplication {

        public Task<ApplicationDetectionResult> DetectAsync()
        {
            using (var key = Registry.ClassesRoot.OpenSubKey(@"SOFTWARE\Deluge", false))
            {
                if (key == null)
                {
                    Trace.TraceInformation("Couldn't find Deluge key at HKCR\\SOFTWARE\\Deluge");
                    return Task.FromResult(ApplicationDetectionResult.NotFound());
                }

                var startMenuFolder = (string)key.GetValue("Start Menu Folder");
                if (string.IsNullOrWhiteSpace(startMenuFolder))
                {
                    Trace.TraceInformation("Couldn't find Deluge start menu location");
                    return Task.FromResult(ApplicationDetectionResult.NotFound());
                }

                // Get the link
                var linkPath = Path.Combine(Environment.ExpandEnvironmentVariables(@"%ALLUSERSPROFILE%\Microsoft\Windows\Start Menu\Programs"), startMenuFolder);
                if (!Directory.Exists(linkPath))
                {
                    Trace.TraceInformation("Couldn't find Deluge shortcut folder: " + linkPath);
                    return Task.FromResult(ApplicationDetectionResult.NotFound());
                }

                var shortcut = Path.Combine(linkPath, "Deluge.lnk");
                if (!File.Exists(shortcut))
                {
                    Trace.TraceInformation("Couldn't find Deluge shortcut: " + shortcut);
                    return Task.FromResult(ApplicationDetectionResult.NotFound());
                }

                var path = ShellLinkHelper.ResolveShortcut(shortcut);
                Trace.TraceInformation("Deluge location is " + path);

                var result = ApplicationDetectionResult.Create(this, "Deluge", Path.GetDirectoryName(path));

                var exe = new FileInfo(path);
                if (!exe.Exists)
                {
                    Trace.TraceInformation("Deluge exe not found @ " + path);
                    result.IsPresent = false;
                }

                var version = FileVersionInfo.GetVersionInfo(exe.FullName);
                result.Description = version.ProductName;
                result.Version = version.FileVersion;

                return Task.FromResult(result);
            }

            // DisplayName: Deluge 1.3.15
            // UninstallString: C:\Program Files (x86)\Deluge\deluge-uninst.exe
            var uninstallKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Deluge";
            var uinstallValue = @"";

            // Recent Apps GUID: {86B4A402-4897-48E8-8D82-0D19C33E1431}
            // AppId: {7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\Deluge\deluge.exe
            // AppPath: C:\Program Files (x86)\Deluge\deluge.exe

        }

        public async Task<FilterUpdateResult> UpdateFilterAsync(FilterDownloadResult filter, CancellationToken cancellationToken, IProgress<ProgressModel> progress)
        {
            var roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
            var destinationPath = Path.Combine(roamingPath, "deluge", "ipfilter.dat");

            Trace.TraceInformation("Writing filter to " + destinationPath);
            using (var destination = File.Open(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new P2pWriter(destination))
            {
                await writer.Write(filter.Entries, progress);
            }

            return new FilterUpdateResult { FilterTimestamp = filter.FilterTimestamp };
        }
    }
}