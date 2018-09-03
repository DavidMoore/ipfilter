using System;
using System.Threading.Tasks;

namespace IPFilter.Core
{
    public interface IFilterWriter : IDisposable
    {
        Task WriteLineAsync(string line);
    }
}