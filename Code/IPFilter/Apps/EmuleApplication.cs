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
        Environment.SpecialFolder configFolder;

        protected virtual string RegistryKeyName => @"Software\eMule";

        protected virtual string DefaultDisplayName => "eMule";

        protected virtual string FolderName => "eMule";
        
        public Task<ApplicationDetectionResult> DetectAsync()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyName, false);
            if (key == null) return Task.FromResult(ApplicationDetectionResult.NotFound());

            var installLocation = (string)key.GetValue("Install Path");
            if (string.IsNullOrWhiteSpace(installLocation)) return Task.FromResult(ApplicationDetectionResult.NotFound());
                
            var result = ApplicationDetectionResult.Create(this, DefaultDisplayName, installLocation);
                
            var exe = new FileInfo(Path.Combine(installLocation, "emule.exe"));
            if (!exe.Exists)
            {
                result.IsPresent = false;
            }
            else
            {
                var version = FileVersionInfo.GetVersionInfo(exe.FullName);
                result.Description = version.ProductName;
                result.Version = version.FileVersion;
            }

            // eMule can be configured to store config in the application folder or program data, instead of app data
            var useSharedConfigValue = key.GetValue("UsePublicUserDirectories") ?? 0;
            configFolder = (int) useSharedConfigValue switch
            {
                1 => Environment.SpecialFolder.CommonApplicationData,
                2 => Environment.SpecialFolder.ProgramFilesX86,
                _ => Environment.SpecialFolder.LocalApplicationData
            };

            return Task.FromResult(result);
        }

        public async Task<FilterUpdateResult> UpdateFilterAsync(FilterDownloadResult filter, CancellationToken cancellationToken, IProgress<ProgressModel> progress)
        {
            var basePath = Environment.GetFolderPath( configFolder, Environment.SpecialFolderOption.Create);
            var destinationPath = Path.Combine(basePath, FolderName, "config", "ipfilter.dat");

            Trace.TraceInformation("Writing filter to " + destinationPath);
            using var destination = File.Open(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new EmuleWriter(destination);
            await writer.Write(filter.Entries, progress);

            return new FilterUpdateResult { FilterTimestamp = filter.FilterTimestamp };
        }
    }
}