using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IPFilter.Core;

namespace IPFilter.Cli
{
    class MultiFilterWriter : IFilterWriter
    {
        readonly IList<IFilterWriter> writers;

        public MultiFilterWriter(IEnumerable<IFilterWriter> writers)
        {
            this.writers = writers.ToList();
        }

        public void Dispose()
        {
            foreach (var writer in writers)
            {
                writer.Dispose();
            }
        }

        public Task WriteLineAsync(string line)
        {
            foreach (var writer in writers)
            {
                writer.WriteLineAsync(line);
            }

            return Task.FromResult(1);
        }

        public async Task Flush()
        {
            foreach (var writer in writers)
            {
                await writer.Flush();
            }
        }
    }
}