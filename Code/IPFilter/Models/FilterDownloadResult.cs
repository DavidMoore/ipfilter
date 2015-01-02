namespace IPFilter.Models
{
    using System;
    using System.IO;
    using System.Net.Http.Headers;

    public class FilterDownloadResult : IDisposable
    {
        public DateTimeOffset? FilterTimestamp { get; set; }

        public IMirrorProvider MirrorProvider { get; set; }

        public string Uri { get; set; }

        public long? Length { get; set; }

        public Exception Exception { get; set; }

        public MemoryStream Stream { get; set; }

        public CompressionFormat CompressionFormat { get; set; }

        public EntityTagHeaderValue Etag { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!disposing || Stream == null) return;
            Stream.Dispose();
        }
    }
}