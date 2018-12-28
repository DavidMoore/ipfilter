using System.IO;
using System.IO.Compression;
using System.Text;

namespace IPFilter.Tests
{
    static class TestFilterData
    {
        public const string TextWithBlankLines = "Line 1\r\n   \r\nLine 2";
        public const string TextMixedLineEndings = " Windows style\r\n   \nLinux style\n";
        public const string TextWithMixedBinary = " Line 1 \0";

        public static MemoryStream CreateStream(string data)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(data));
        }

        public static MemoryStream GetGZipStream(string data)
        {
            var stream = new MemoryStream();
            
            // Write out the test data
            using (var zip = StreamHelper.CreateGZipStream(stream, CompressionMode.Compress))
            using (var writer = StreamHelper.CreateStreamWriter(zip))
            {
                writer.Write(data);
            }

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}