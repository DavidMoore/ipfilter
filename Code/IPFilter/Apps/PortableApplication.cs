namespace IPFilter.Apps
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    /// <summary>
    /// Auto-detects running portable versions of BitTorrent clients
    /// </summary>
    class PortableApplication : IApplication
    {
        static readonly IList<string> portableNames = new[] {
            "qBitTorrentPortable",
        }.Select( x => x.ToLowerInvariant()).ToList();

        protected IList<string> Destinations;

        public Task<ApplicationDetectionResult> DetectAsync()
        {
            var processes = Process.GetProcesses();

            foreach(var process in processes)
            {
                Trace.TraceInformation(process.ProcessName);
            }

            return Task.FromResult(ApplicationDetectionResult.NotFound());
        }

        public Task<FilterUpdateResult> UpdateFilterAsync(FilterDownloadResult filter, CancellationToken cancellationToken, IProgress<ProgressModel> progress)
        {
            throw new NotImplementedException();
        }
    }
}