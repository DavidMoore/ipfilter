namespace IPFilter
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UI.Apps;

    public class ApplicationEnumerator
    {
        public async Task<IEnumerable<ApplicationDetectionResult>> GetInstalledApplications()
        {
            var results = new List<ApplicationDetectionResult>();

            var applications = new List<IApplication>
            {
                new QBitTorrent(), 
                new UTorrentApplication(),
                new BitTorrentApplication()
            };

            foreach (var application in applications)
            {
                var installed = await application.DetectAsync();

                if (installed != null && installed.IsPresent)
                {
                    results.Add(installed);
                }
            }

            return results;
        }
    }
}