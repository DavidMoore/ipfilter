using System;
using System.Threading;

namespace IPFilter.Core
{
    public class FilterContext : IDisposable
    {
        public FilterContext()
        {
            UriResolver = new UriResolver();
            Filter = new NullWriter();
            FileSystem = new FileSystem();
        }

        public virtual CancellationToken CancellationToken { get; set; }

        //public IProgress<ProgressModel> Progress { get; set; }

        public IUriResolver UriResolver { get; set; }

        public IFilterWriter Filter { get; set; }

        public IFileSystem FileSystem { get; set; }

        public void Dispose()
        {
            Filter?.Dispose();
        }
    }
}