namespace IPFilter
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    class FileDownloader : IFileDownloader
    {
        public async Task<Stream> DownloadAsync(Uri uri, CancellationToken cancellationToken, IProgress<int> progress)
        {
            if (cancellationToken.IsCancellationRequested) return null;

            using (var handler = new WebRequestHandler())
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                
                using (var httpClient = new HttpClient(handler))
                using (var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested) return null;
                    return await response.Content.ReadAsStreamAsync();
                }
            }
        }
    }

    interface IFileDownloader
    {
        Task<Stream> DownloadAsync(Uri uri, CancellationToken cancellationToken, IProgress<int> progress);
    }
}
