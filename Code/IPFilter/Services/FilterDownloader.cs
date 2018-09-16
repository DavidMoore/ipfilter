namespace IPFilter.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using ListProviders;
    using Models;
    using Properties;

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
            Mirrors = new List<IMirrorProvider> { new DefaultList() };
        }
        
        public async Task<FilterDownloadResult> DownloadFilter(Uri uri, CancellationToken cancellationToken, IProgress<ProgressModel> progress)
        {
            var result = new FilterDownloadResult();

            try
            {
                
                if (uri == null)
                {
                    var provider = Mirrors.First();
                    uri = new Uri(provider.GetUrlForMirror());
                }

                result.Uri = uri.ToString();

                using (var handler = new WebRequestHandler())
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                    Trace.TraceInformation("Downloading filter from " + result.Uri);

                    using (var httpClient = new HttpClient(handler))
                    using (var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                    {
                        if (cancellationToken.IsCancellationRequested) return null;

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException( (int)response.StatusCode + ": " + response.ReasonPhrase);
                        }

                        result.FilterTimestamp = response.Content.Headers.LastModified;
                        result.Etag = response.Headers.ETag;
                        result.Length = response.Content.Headers.ContentLength;

                        Trace.TraceInformation("Online filter's timestamp is " + result.FilterTimestamp);
                        Trace.TraceInformation("ETag: '{0}'", result.Etag);
                        
                        // Check if the cached filter is already up to date.
                        if (cache != null && !Settings.Default.DisableCache && result.Etag != null && !result.Etag.IsWeak)
                        {
                            var cacheResult = await cache.GetAsync(result);

                            if (cacheResult != null && cacheResult.Length > 0)
                            {
                                Trace.TraceInformation("Found cached ipfilter with timestamp of " + cacheResult.FilterTimestamp);
                                if (cacheResult.FilterTimestamp >= result.FilterTimestamp && cacheResult.Etag != null && !cacheResult.Etag.IsWeak && cacheResult.Etag.Tag == result.Etag.Tag)
                                {
                                    Trace.TraceInformation("Using the cached ipfilter as it's the same or newer than the online filter.");
                                    return cacheResult;
                                }
                            }
                        }
                        

                        double lengthInMb = !result.Length.HasValue ? -1 : (double) result.Length.Value / 1024 / 1024;

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
                                    double downloadedMegs = bytesDownloaded / 1024 / 1024;
                                    var percent = (int)Math.Floor((bytesDownloaded / result.Length.Value) * 100);
                                    
                                    var status = string.Format(CultureInfo.CurrentUICulture, "Downloaded {0:F2} MB of {1:F2} MB", downloadedMegs, lengthInMb);

                                    progress.Report(new ProgressModel(UpdateState.Downloading, status, percent));
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
                    result.Stream = await Decompress(result, cancellationToken, progress);
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

        async Task<MemoryStream> Decompress(FilterDownloadResult filter, CancellationToken cancellationToken, IProgress<ProgressModel> progress)
        {
            using (var stream = filter.Stream)
            {
                var result = new MemoryStream();

                stream.Seek(0, SeekOrigin.Begin);

                cancellationToken.ThrowIfCancellationRequested();

                switch (filter.CompressionFormat)
                {
                    case CompressionFormat.GZip:
                        using(var gzipFile = new GZipStream(stream, CompressionMode.Decompress))
                        {
                            var buffer = new byte[1024 * 64];
                            int bytesRead;
                            while ((bytesRead = await gzipFile.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                result.Write(buffer, 0, bytesRead);
                                progress.Report(new ProgressModel(UpdateState.Decompressing, "Decompressing...", -1));
                            }
                        }
                        break;

                    case CompressionFormat.Zip:

                        using (var zipFile = new ZipArchive(stream,ZipArchiveMode.Read))
                        {
                            progress.Report(new ProgressModel(UpdateState.Decompressing, "Decompressing...", -1));
                            
                            if (zipFile.Entries.Count == 0) throw new IOException("There are no entries in the zip file.");
                            if (zipFile.Entries.Count > 1) throw new IOException("There is more than one file in the zip file. This application will need to be updated to support this.");

                            var entry = zipFile.Entries.First();

                            using (var entryStream = entry.Open())
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                await entryStream.CopyToAsync(result);
                            }
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