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
    /// Writes out eMule DAT format, 0-padding integers to 3 digits. e.g.<c>"001.009.096.105 - 001.009.096.105 , 000 , Some organization"</c>
    /// </summary>
    class BitTorrentWriter : IDisposable
    {
        readonly Stream stream;

        public void Dispose()
        {
            stream.Dispose();
        }

        public BitTorrentWriter(Stream stream)
        {
            this.stream = stream;
        }

        public async Task Write(IList<FilterEntry> entries, IProgress<ProgressModel> progress)
        {
            var sb = new StringBuilder(255);
            var address = new StringBuilder(15);

            var currentPercentage = -1;

            using (Benchmark.New("Writing {0} entries", entries.Count))
            using (var writer = new StreamWriter(stream, Encoding.ASCII))
            {
                for (var i = 1; i <= entries.Count; i++)
                {
                    sb.Clear();

                    var from = BitConverter.GetBytes(entries[i - 1].From);
                    address.Clear();
                    address.Append(from[3].ToString("D3")).Append(".").Append(from[2].ToString("D3")).Append(".").Append(from[1].ToString("D3")).Append(".").Append(from[0].ToString("D3"));
                    sb.Append(address);

                    sb.Append(" - ");

                    var to = BitConverter.GetBytes(entries[i - 1].To);
                    address.Clear();
                    address.Append(to[3].ToString("D3")).Append(".").Append(to[2].ToString("D3")).Append(".").Append(to[1].ToString("D3")).Append(".").Append(to[0].ToString("D3"));
                    sb.Append(address);

                    sb.Append(" , ").Append(entries[i - 1].Level.ToString("D3").PadLeft(3)).Append(" , ");

                    sb.Append(entries[i - 1].Description);

                    await writer.WriteLineAsync(sb.ToString());

                    if (progress == null) continue;
                    var percent = (int)Math.Floor((double)i / entries.Count * 100);

                    if (percent > currentPercentage)
                    {
                        progress.Report(new ProgressModel(UpdateState.Downloading, "Writing...", percent));
                    }

                    currentPercentage = percent;
                }
            }
        }
    }
}