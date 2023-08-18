using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using IPFilter.Cli;
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
                if (exe.Exists)
                {
                    var version = FileVersionInfo.GetVersionInfo(exe.FullName);
                    result.Description = version.ProductName;
                    result.Version = version.FileVersion;
                }
                else
                {
                    Trace.TraceInformation("Deluge exe not found @ " + path);
                    result.IsPresent = false;
                }

                return Task.FromResult(result);
            }

            // DisplayName: Deluge 1.3.15
            // UninstallString: C:\Program Files (x86)\Deluge\deluge-uninst.exe
            var uninstallKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Deluge";
            var uinstallValue = @"";

            // Recent Apps GUID: {86B4A402-4897-48E8-8D82-0D19C33E1431}
            // AppId: {7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E}\Deluge\deluge.exe
            // AppPath: C:\Program Files (x86)\Deluge\deluge.exe

            // blocklist.conf

        }

        public async Task<FilterUpdateResult> UpdateFilterAsync(FilterDownloadResult filter, CancellationToken cancellationToken, IProgress<ProgressModel> progress)
        {
            var roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
            var destination = new FileInfo(Path.Combine(roamingPath, "deluge", "ipfilter.dat"));

            Trace.TraceInformation("Writing filter to " + destination.FullName);
            if(!destination.Directory.Exists) destination.Directory.Create();
            
            using (var stream = File.Open(destination.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new P2pWriter(stream))
            {
                await writer.Write(filter.Entries, progress);
            }

            // Get the blocklist config file
            var blocklistConfigFile = new FileInfo( Path.Combine( destination.DirectoryName, "blocklist.conf") );
            var builder = new UriBuilder(destination.FullName);

            var config = new DelugeBlocklistConfig();
            var header = new DelugeConfigHeader();

            if (blocklistConfigFile.Exists)
            {
                var blocklistConfigValue = await blocklistConfigFile.ReadAllText();

                var headerMarker = blocklistConfigValue.IndexOf('}');

                header = serializer.Deserialize<DelugeConfigHeader>(blocklistConfigValue.Substring(0, headerMarker + 1));
                config = serializer.Deserialize<DelugeBlocklistConfig>( blocklistConfigValue.Substring(headerMarker + 1) );
            }

            // Update blocklist config
            config.url = builder.Uri.AbsolutePath;
            config.list_type = "Emule";
            config.load_on_start = true;
            config.list_size = (int)(filter.Length ?? 0);
            config.last_update = filter.FilterTimestamp.HasValue ? filter.FilterTimestamp.Value.ToUnixTimeSeconds() : 0;

            // Write out blocklist config
            var headerJson = serializer.Serialize(header);
            var configJson = serializer.Serialize(config);

            await blocklistConfigFile.WriteAllText(headerJson + configJson);

            return new FilterUpdateResult { FilterTimestamp = filter.FilterTimestamp };
        }

        internal static readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        class DelugeConfigHeader
        {
            public int file { get; set; } = 1;
            public int format { get; set; } = 1;
        }

        class DelugeBlocklistConfig
        {
            public int check_after_days { get; set; }
            
            public float last_update { get; set; }

            public string list_compression { get; set; }

            public int list_size { get; set; }

            public string list_type { get; set; }

            public int timeout { get; set; }

            public int try_times { get; set; }

            public bool load_on_start { get; set; }

            public string[] whitelisted { get; set; }

            public string url { get; set; }

        }

        // deluge-debug.exe -L debug -l C:\Users\<use>\AppData\Roaming\deluge\deluge.log
        // https://github.com/deluge-torrent/deluge/blob/develop/deluge/plugins/Blocklist/deluge_blocklist/
        // blocklist.conf
        //        {
        //    "file": 1,
        //    "format": 1
        //}{
        //    "check_after_days": 4,
        //    "last_update": 0.0,
        //    "list_compression": "",
        //    "list_size": 0,
        //    "list_type": "Emule",
        //    "load_on_start": false,
        //    "timeout": 180,
        //    "try_times": 3,
        //    "url": "ipfilter.dat",
        //    "whitelisted": []
        //}

        // Core.conf
        // "enabled_plugins": [
        // "Blocklist"],
    }
}