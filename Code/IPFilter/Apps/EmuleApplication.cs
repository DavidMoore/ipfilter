using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IPFilter.Formats;
using IPFilter.Models;
using Microsoft.Win32;

namespace IPFilter.Apps
{
    class EmuleApplication : IApplication
    {
        protected virtual string RegistryKeyName => @"Software\eMule";

        protected virtual string DefaultDisplayName => "eMule";

        protected virtual string FolderName => "eMule";

        public Task<ApplicationDetectionResult> DetectAsync()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RegistryKeyName, false))
            {
                if (key == null) return Task.FromResult(ApplicationDetectionResult.NotFound());

                var installLocation = (string)key.GetValue("Install Path");
                if (installLocation == null) return Task.FromResult(ApplicationDetectionResult.NotFound());
                
                var result = ApplicationDetectionResult.Create(this, DefaultDisplayName, installLocation);
                
                var exe = new FileInfo(Path.Combine(installLocation, "emule.exe"));
                if (!exe.Exists) result.IsPresent = false;

                var version = FileVersionInfo.GetVersionInfo(exe.FullName);
                result.Description = version.ProductName;
                result.Version = version.FileVersion;
                
                return Task.FromResult(result);
            }
        }

        public async Task<FilterUpdateResult> UpdateFilterAsync(FilterDownloadResult filter, CancellationToken cancellationToken, IProgress<ProgressModel> progress)
        {
            var roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
            var destinationPath = Path.Combine(roamingPath, FolderName, "config", "ipfilter.dat");

            Trace.TraceInformation("Writing filter to " + destinationPath);
            using (var destination = File.Open(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using(var writer = new EmuleWriter(destination))
            {
                await writer.Write(filter.Entries, progress);
            }

            return new FilterUpdateResult { FilterTimestamp = filter.FilterTimestamp };
        }
    }
}