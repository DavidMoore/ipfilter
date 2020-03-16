using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IPFilter.Core;
using IPFilter.Models;

namespace IPFilter.Formats
{
    /// <summary>
    /// Writes out ipfilter.dat for eMule, which aligns the data in space-padded columns e.g.<c>"1.2.8.0         - 1.2.8.255       ,   0 ,  Some organization"</c>
    /// </summary>
    class EmuleWriter : IFormatWriter
    {
        readonly Stream stream;

        public void Dispose()
        {
            stream.Dispose();
        }

        public EmuleWriter(Stream stream)
        {
            this.stream = stream;
        }

        public async Task Write(IList<FilterEntry> entries, IProgress<ProgressModel> progress)
        {
            var sb = new StringBuilder(255);
            var address = new StringBuilder(15);

            using(Benchmark.New("Writing {0} entries", entries.Count))
            using (var writer = new StreamWriter(stream, Encoding.ASCII))
            {
                for (var i = 1; i <= entries.Count; i++)
                {
                    var entry = entries[i-1];
                    sb.Clear();

                    var from = BitConverter.GetBytes(entry.From);
                    address.Clear();
                    address.Append(from[3]).Append(".").Append(from[2]).Append(".").Append(from[1]).Append(".").Append(from[0]);
                    sb.Append(address.ToString().PadRight(16));

                    sb.Append("- ");

                    var to = BitConverter.GetBytes(entry.To);
                    address.Clear();
                    address.Append(to[3]).Append(".").Append(to[2]).Append(".").Append(to[1]).Append(".").Append(to[0]);
                    sb.Append(address.ToString().PadRight(16));

                    sb.Append(", ").Append(entry.Level.ToString().PadLeft(3)).Append(" , ");

                    sb.Append(entry.Description);

                    writer.WriteLine(sb.ToString());

                    if (progress == null) continue;
                    var percent = (int) Math.Floor((double) (i / entries.Count * 100));
                    progress.Report(new ProgressModel(UpdateState.Decompressing, "Updating eMule...", percent));
                }

                progress?.Report(new ProgressModel(UpdateState.Decompressing, "Flushing...", 100));
                await writer.FlushAsync();

                progress?.Report(new ProgressModel(UpdateState.Decompressing, "Updated eMule.", 100));
            }
        }
    }
}