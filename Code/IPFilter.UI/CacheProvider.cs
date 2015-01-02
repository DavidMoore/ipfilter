namespace IPFilter
{
    using System;
    using System.Deployment.Application;
    using System.IO;
    using System.Threading.Tasks;
    using Models;

    class CacheProvider : ICacheProvider
    {
        static string dataPath;
        static readonly string filterPath;

        static CacheProvider()
        {
            dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DavidMoore", "IPFilter");

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                dataPath = ApplicationDeployment.CurrentDeployment.DataDirectory;
            }

            filterPath = Path.Combine(dataPath, "ipfilter.dat");
        }
        
        public static string FilterPath
        {
            get { return filterPath; }
        }

        public FilterDownloadResult Get(FilterDownloadResult filter)
        {
            var file = new FileInfo(filterPath);

            if (!file.Exists) return null;

            var result = new FilterDownloadResult();

            result.FilterTimestamp = file.LastWriteTimeUtc;

            result.Stream = new MemoryStream((int) file.Length);

            using (var stream = file.OpenRead())
            {
                stream.CopyToAsync(result.Stream);
            }

            result.Length = result.Stream.Length;

            return result;
        }

        public async Task SetAsync(FilterDownloadResult filter)
        {
            if (filter == null || filter.Exception != null) return;

            var file = new FileInfo(filterPath);

            if (file.Directory != null && !file.Directory.Exists)
            {
                file.Directory.Create();
            }

            using (var cacheFile = File.Open(filterPath, FileMode.Create, FileAccess.Write,FileShare.Read))
            {
                filter.Stream.WriteTo(cacheFile);
            }

            if (filter.FilterTimestamp != null)
            {
                file.LastWriteTimeUtc = filter.FilterTimestamp.Value.UtcDateTime;
            }
        }
    }
}