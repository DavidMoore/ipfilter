using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IPFilter.Formats;
using IPFilter.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IPFilter.Tests.Formats
{
    [TestClass]
    public class P2pWriterTests
    {
        [TestMethod]
        public async Task Write()
        {
            var entries = new List<FilterEntry>
            {
                new FilterEntry(0x01020304, 0x010203FF)
                {
                    Description = "Testing the description"
                }
            };

            using (var stream = new MemoryStream())
            {
                using (var writer = new P2pWriter(stream))
                {
                    var progress = new Mock<IProgress<ProgressModel>>();
                    await writer.Write(entries, progress.Object);
                }

                Assert.AreEqual("001.002.003.004 - 001.002.003.255 , 000 , Testing the description\r\n", Encoding.ASCII.GetString(stream.ToArray()));
            }
        }
    }
}