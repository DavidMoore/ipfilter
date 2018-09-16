using System;
using System.Threading.Tasks;

namespace IPFilter.Core
{
    public interface IFilterWriter : IDisposable
    {
        Task WriteLineAsync(string line);

        /// <summary>
        /// Finalizes and flushes the list to output. Typically this will
        /// be the file system, and will involve sorting the list and removing duplicates.
        /// </summary>
        Task Flush();
    }
}