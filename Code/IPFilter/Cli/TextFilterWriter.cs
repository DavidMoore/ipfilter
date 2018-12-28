using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using IPFilter.Core;
using IPFilter.Formats;
using IPFilter.Models;

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
            var parsed = DatParser.ParseLine(line);
            if (parsed == null)
            {
                Trace.TraceWarning("Invalid line: " + line);
                return;
            }

            await writer.WriteLineAsync(parsed);
        }

        public async Task Flush()
        {
            await writer.FlushAsync();
            writer.Dispose();

            var filters = new List<FilterEntry>();
            
            using(var input = new StreamReader(temp.OpenShareableRead()))
            {
                string line;
                while ((line = await input.ReadLineAsync()) != null)
                {
                    var filter = DatParser.ParseEntry(line);
                    if( filter != null) filters.Add(filter);
                }
            }

            // Sort and merge the list
            var list = FilterCollection.Merge(filters);

            // Flush the list out
            using (var stream = file.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var listWriter = new EmuleWriter(stream))
            {
                await listWriter.Write(list, null);
            }
        }
    }
}