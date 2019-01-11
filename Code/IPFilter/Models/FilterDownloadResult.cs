using System.Collections.Generic;

namespace IPFilter.Models
{
    using System;
    using System.IO;
    using System.Net.Http.Headers;
    using ListProviders;

    public class FilterDownloadResult : IDisposable
    {
        public FilterDownloadResult()
        {
            Entries = new List<FilterEntry>(100000);
        }

        public DateTimeOffset? FilterTimestamp { get; set; }

        public IMirrorProvider MirrorProvider { get; set; }

        public string Uri { get; set; }

        public long? Length { get; set; }

        public Exception Exception { get; set; }

        public MemoryStream Stream { get; set; }

        public CompressionFormat CompressionFormat { get; set; }

        public EntityTagHeaderValue Etag { get; set; }

        public IList<FilterEntry> Entries { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (!disposing || Stream == null) return;
            Stream.Dispose();
        }
    }
}