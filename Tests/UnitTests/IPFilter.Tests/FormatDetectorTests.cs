using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using IPFilter.Cli;
using IPFilter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPFilter.Tests
{
    [TestClass]
    public class FormatDetectorTests
    {
        [TestMethod]
        public async Task GZip()
        {
            // Write test GZip stream
            using (var stream = TestFilterData.GetGZipStream("testing"))
            {
                stream.Seek(0, SeekOrigin.Begin);
                var format = await FormatDetector.DetectFormat(stream);
                Assert.AreEqual(DataFormat.GZip, format);
            }
        }

        [TestMethod]
        public async Task Zip()
        {
            using (var stream = new MemoryStream())
            {
                using (var zip = StreamHelper.CreateZipArchive(stream, ZipArchiveMode.Create))
                {
                    StreamHelper.CreateZipArchiveEntry(zip, "text.txt", TestFilterData.TextWithBlankLines);
                }

                stream.Seek(0, SeekOrigin.Begin);
                var format = await FormatDetector.GetFormat(stream);
                Assert.AreEqual(DataFormat.Zip, format );
            }
        }

        [TestMethod]
        public async Task Json()
        {
            using (var stream = TestFilterData.CreateStream("{json: true}"))
            {
                stream.Seek(0, SeekOrigin.Begin);
                var format = await FormatDetector.GetFormat(stream);
                Assert.AreEqual(DataFormat.Json, format);
            }

            var json = "\r\n{ lists: [{ \r\n    lists: [\r\n        { uri: \"http://localhost/ipfilter.dat\"}\r\n    ] \r\n}]}";

            using (var stream = TestFilterData.CreateStream(json))
            {
                stream.Seek(0, SeekOrigin.Begin);
                var format = await FormatDetector.GetFormat(stream);
                Assert.AreEqual(DataFormat.Json, format);
            }
        }
    }
}