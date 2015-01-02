namespace IPFilter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.AccessControl;
    using System.Threading;
    using System.Threading.Tasks;
    using Ionic.Zip;
    using Models;
    using UI.ListProviders;

    public class FilterDownloader
    {
        public IEnumerable<IMirrorProvider> Mirrors { get; set; }

        readonly ICacheProvider cache;

        public FilterDownloader() : this(new CacheProvider())
        {
        }

        public FilterDownloader(ICacheProvider cache)
        {
            this.cache = cache;
            Mirrors = new List<IMirrorProvider> { new EmuleSecurity(), new BlocklistMirrorProvider() };
        }
        
        public async Task<FilterDownloadResult> DownloadFilter(Uri uri, CancellationToken cancellationToken, IProgress<int> progress)
        {
            var result = new FilterDownloadResult();

            try
            {
                if (uri == null)
                {
                    var provider = Mirrors.First();
                    var mirror = provider.GetMirrors().First();
                    uri = new Uri(provider.GetUrlForMirror(mirror));
                }

                result.Uri = uri.ToString();

                using (var handler = new WebRequestHandler())
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                    using (var httpClient = new HttpClient(handler))
                    using (var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                    {
                        if (cancellationToken.IsCancellationRequested) return null;

                        result.FilterTimestamp = response.Headers.Date;
                        result.Etag = response.Headers.ETag;

                        // Check if the cached filter is already up to date.
                        if (cache != null)
                        {
                            var cacheResult = cache.Get(result);

                            if (cacheResult != null && cacheResult.Length > 0)
                            {
                                return cacheResult;
                            }
                        }
                        
                        result.Length = response.Content.Headers.ContentLength;

                        double bytesDownloaded = 0;

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var buffer = new byte[65535 * 4];

                            result.Stream = new MemoryStream((int) (result.Length ?? buffer.Length));

                            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                            result.CompressionFormat = DetectCompressionFormat(buffer, response.Content.Headers.ContentType);

                            while (bytesRead != 0)
                            {
                                await result.Stream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                                bytesDownloaded += bytesRead;

                                if (result.Length.HasValue)
                                {
                                    var percent = (int) Math.Floor((bytesDownloaded/result.Length.Value)*100);
                                    progress.Report(percent);
                                }

                                if (cancellationToken.IsCancellationRequested) return null;

                                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                            }
                        }
                    }
                }

                // Decompress if necessary
                if (result.CompressionFormat != CompressionFormat.None)
                {
                    result.Stream = await Decompress(result,cancellationToken, progress);
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                return result;
            }

            // Write to cache
            if (cache != null)
            {
                await cache.SetAsync(result);
            }

            return result;
        }

        async Task<MemoryStream> Decompress(FilterDownloadResult filter, CancellationToken cancellationToken, IProgress<int> progress)
        {
            using (var stream = filter.Stream)
            {
                var result = new MemoryStream();

                stream.Seek(0, SeekOrigin.Begin);

                switch (filter.CompressionFormat)
                {
                    case CompressionFormat.GZip:
                        using(var gzipFile = new GZipStream(stream, CompressionMode.Decompress))
                        {
                            var buffer = new byte[1024 * 64];
                            int bytesRead = 0;
                            while ((bytesRead = await gzipFile.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                            {
                                result.Write(buffer, 0, bytesRead);
                            }
                        }
                        break;

                    case CompressionFormat.Zip:

                        EventHandler<ReadProgressEventArgs> reportProgress = (sender, args) =>
                        {
                            if (args.EventType != ZipProgressEventType.Extracting_EntryBytesWritten) return;
                            var percentage = args.BytesTransferred/args.TotalBytesToTransfer*100;
                            progress.Report((int) percentage);
                        };

                        using (var zipFile = ZipFile.Read(stream, reportProgress))
                        {
                            if (zipFile.Entries.Count == 0) throw new ZipException("There are no entries in the zip file.");
                            if (zipFile.Entries.Count > 1) throw new ZipException("There is more than one file in the zip file. This application will need to be updated to support this.");

                            var entry = zipFile.Entries.First();

                            entry.Extract(result);
                        }
                        break;

                    default:
                        await stream.CopyToAsync(result);
                        break;
                }

                return result;
            }
        }

        CompressionFormat DetectCompressionFormat(byte[] buffer, MediaTypeHeaderValue contentType)
        {
            switch (contentType.MediaType)
            {
                case "application/gzip":
                case "application/x-gzip":
                case "application/x-gunzip":
                case "application/gzipped":
                case "application/gzip-compressed":
                case "gzip/document":
                    return CompressionFormat.GZip;

                case "application/zip":
                case "application/x-zip":
                case "application/x-zip-compressed":
                case "multipart/x-zip":
                    return CompressionFormat.Zip;

//                case "application/x-compressed":
//                case "application/octet-stream":
//                case "text/plain":
                default:
                    {
                        // Look for the GZip header bytes
                        if (buffer[0] == 31 && buffer[1] == 139)
                        {
                            return CompressionFormat.GZip;
                        }

                        // Look for the ZIP header bytes.
                        var zipHeaderNumber = BitConverter.ToInt32(buffer, 0);
                        if (zipHeaderNumber == 0x4034b50)
                        {
                            return CompressionFormat.Zip;
                        }
                    }
                    break;
            }

            return CompressionFormat.None;
        }
    }
}