using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IPFilter.Models;

namespace IPFilter.Formats
{
    interface IFormatWriter : IDisposable
    {
        Task Write(IList<FilterEntry> entries, IProgress<ProgressModel> progress);
    }
}