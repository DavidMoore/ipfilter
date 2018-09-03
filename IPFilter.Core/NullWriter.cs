using System.Threading.Tasks;

namespace IPFilter.Core
{
    public class NullWriter : IFilterWriter
    {
        static readonly Task empty = Task.FromResult(1);

        public Task WriteLineAsync(string line)
        {
            return empty;
        }

        public void Dispose()
        {
        }
    }
}