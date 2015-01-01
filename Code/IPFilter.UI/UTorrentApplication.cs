namespace IPFilter
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Win32;

    class UTorrentApplication : IApplication
    {
        public async Task<ApplicationDetectionResult> DetectAsync()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\uTorrent", false))
            {
                if (key == null) return ApplicationDetectionResult.NotFound();

                var installLocation = (string) key.GetValue("InstallLocation");
                if (installLocation == null) return ApplicationDetectionResult.NotFound();

                var displayName = (string)key.GetValue("DisplayName") ?? "uTorrent";
                var version = (string)key.GetValue("DisplayVersion") ?? "Unknown";

                var result = new ApplicationDetectionResult()
                {
                    IsPresent = true,
                    Description = displayName,
                    InstallLocation = new DirectoryInfo(installLocation),
                    Version = version
                };

                if (!result.InstallLocation.Exists) result.IsPresent = false;

                return result;
            }
        }

        public async Task<FilterUpdateResult> UpdateFilterAsync(IProgress<int> progress)
        {
            return null;
        }
    }
}