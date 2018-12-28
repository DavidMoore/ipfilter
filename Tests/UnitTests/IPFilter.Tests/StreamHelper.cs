using System.IO;
using System.IO.Compression;
using System.Text;

namespace IPFilter.Tests
{
    public static class StreamHelper
    {
        static int DefaultBufferSize = 65535;

        public static GZipStream CreateGZipStream(Stream stream, CompressionMode compressionMode)
        {
            return new GZipStream(stream, compressionMode, true);
        }

        public static StreamWriter CreateStreamWriter(Stream stream)
        {
            return new StreamWriter(stream, Encoding.UTF8, DefaultBufferSize, true);
        }

        public static StreamReader CreateStreamReader(Stream stream)
        {
            return new StreamReader(stream, Encoding.UTF8, false, DefaultBufferSize, true);
        }

        public static ZipArchive CreateZipArchive(Stream stream, ZipArchiveMode mode)
        {
            return new ZipArchive(stream, mode, true, Encoding.UTF8);
        }

        public static ZipArchiveEntry CreateZipArchiveEntry(ZipArchive archive, string name, string data)
        {
            var entry = archive.CreateEntry(name);
            using(var stream = entry.Open())
            using (var writer = CreateStreamWriter(stream))
            {
                writer.Write(data);
            }

            return entry;
        }
    }
}