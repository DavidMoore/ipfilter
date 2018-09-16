using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPFilter.Core;

namespace IPFilter.Cli
{
    class TextFilterWriter : IFilterWriter
    {
        readonly FileInfo file;
        readonly TextWriter writer;
        readonly TempFile temp;

        public TextFilterWriter(string path)
        {
            file = new FileInfo(path);
            temp = new TempFile();
            
            writer = new StreamWriter( temp.File.Open(FileMode.Create, FileAccess.Write, FileShare.Read));
        }

        public void Dispose()
        {
            writer?.Dispose();
            temp?.Dispose();
        }

        public async Task WriteLineAsync(string line)
        {
            await writer.WriteLineAsync(line);
        }

        public async Task Flush()
        {
            await writer.FlushAsync();
            writer.Dispose();

            var lines = new List<string>();

            using(var input = new StreamReader(temp.OpenShareableRead()))
            {
                string line;
                while ((line = await input.ReadLineAsync()) != null)
                {
                    lines.Add(line);
                }
            }

            using (var finalWriter = new StreamWriter(file.Open(FileMode.Create, FileAccess.Write, FileShare.Read)))
            {
                foreach (var line in lines.OrderBy(x => x).Distinct())
                {
                    await finalWriter.WriteLineAsync(line);
                }
            }
        }
    }
}