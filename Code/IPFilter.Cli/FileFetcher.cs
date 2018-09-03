using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IPFilter.Core;

namespace IPFilter.Cli
{
    class FileFetcher : IDisposable
    {
        static readonly HttpClient client;
        
        static FileFetcher()
        {
            var handler = new WebRequestHandler();
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            client = new HttpClient(handler);
        }
        
        public virtual async Task<FileNode> Get(Uri uri, FilterContext context)
        {
            var tempFile = new FileInfo(Path.GetTempFileName());

            try
            {
                if (uri.IsFile)
                {
                    return await GetLocalFile(uri, context, tempFile);
                }

                return await GetRemoteFile(uri, context, tempFile);
            }
            catch (Exception)
            {
                tempFile.SafeDelete();
                throw;
            }
        }

        protected async Task<FileNode> GetRemoteFile(Uri uri, FilterContext context, FileInfo destination)
        {
            using (var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, context.CancellationToken))
            {
                if (context.CancellationToken.IsCancellationRequested) return null;

                var sourceTimestamp = response.Content.Headers.LastModified;// ?? response.Headers.Date;
                var length = response.Content.Headers.ContentLength;
                var etag = response.Headers.ETag?.Tag;

                // Check if the etag is matching
                var etagFile = new FileInfo( destination.FullName + ".etag");
                if (etag != null && etagFile.Exists)
                {
                    var existingEtag = (await etagFile.ReadAllText()).Trim();
                    if (existingEtag.Equals(etag, StringComparison.OrdinalIgnoreCase) && destination.Exists)
                    {
                        // We already have the latest version
                        return new FileNode(destination);
                    }
                }

                // Check if the destination file is already up to date.
                if (destination.Exists && (length.HasValue && length.Value == destination.Length) &&
                    (sourceTimestamp.HasValue && sourceTimestamp.Value.UtcDateTime <= destination.LastWriteTimeUtc))
                {
                    // We already have the latest version of the file, so there's
                    // no need to re-download it.
                    return new FileNode(destination);
                }
                
                double lengthInMb = !length.HasValue ? -1 : (double) length.Value / 1024 / 1024;
                double bytesDownloaded = 0;

                using (var source = await response.Content.ReadAsStreamAsync())
                using (var stream = File.Open(destination.FullName, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    await source.CopyToAsync( stream, context.CancellationToken);
                    
                    //if (length.HasValue)
                    //{
//                            double downloadedMegs = bytesDownloaded / 1024 / 1024;
//                            var percent = (int) Math.Floor((bytesDownloaded / length.Value) * 100);
//
//                            var status = string.Format(CultureInfo.CurrentUICulture,
//                                "Downloaded {0:F2} MB of {1:F2} MB", downloadedMegs, lengthInMb);

                    //progress.Report(new ProgressModel(UpdateState.Downloading, status, percent));
                    //}
                }

                // Update the timestamps on the destination, so they reflect
                // the timestamps of the soure file.
                if( sourceTimestamp != null) destination.LastWriteTimeUtc = sourceTimestamp.Value.UtcDateTime;

                destination.Refresh();
                if (length.HasValue)
                {
                    if (destination.Length != length.Value)
                    {
                        Trace.TraceWarning($"Destination expected to be {length.Value} bytes, but was {destination.Length}");
                    }
                }

                // Store etag if available
                if (!string.IsNullOrWhiteSpace(etag)) await etagFile.WriteAllText(etag);

                return new FileNode(destination);
            }
        }

        protected async Task<FileNode> GetLocalFile(Uri uri, FilterContext context,  FileInfo destination)
        {
            var source = new FileInfo(uri.LocalPath);

            if (destination.Exists && destination.LastWriteTimeUtc >= source.LastWriteTimeUtc && destination.Length == source.Length)
            {
                // If we already have an up to date version, use that.
                return new FileNode(destination);
            }

            using (var writer = destination.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var reader = source.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await reader.CopyToAsync(writer, StreamExtensions.DefaultCopyBufferSize, context.CancellationToken);
            }

            // Update the timestamps on the destination, so they reflect
            // the timestamps of the soure file.
            destination.LastWriteTimeUtc = source.LastWriteTimeUtc;

            destination.Refresh();

            Debug.Assert(destination.Length == source.Length);

            return new FileNode(destination);
        }

        public virtual void Dispose()
        {
            client?.Dispose();
        }
    }
}