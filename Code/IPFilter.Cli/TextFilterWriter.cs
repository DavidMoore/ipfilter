using System.IO;
using System.Text;
using System.Threading.Tasks;
using IPFilter.Core;

namespace IPFilter.Cli
{
    class TextFilterWriter : IFilterWriter
    {
        readonly FileInfo file;
        readonly TextWriter writer;

        public TextFilterWriter(string path)
        {
            file = new FileInfo(path);
            writer = new StreamWriter( file.Open(FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8);
        }

        public void Dispose()
        {
            writer?.Dispose();
        }

        public async Task WriteLineAsync(string line)
        {
            await writer.WriteLineAsync(line);
        }
    }
}