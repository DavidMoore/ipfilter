using IPFilter.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPFilter.Tests.Formats
{
    [TestClass]
    public class DatParserTests
    {
        [TestMethod]
        public void ParseLine()
        {
            Assert.IsNull(DatParser.ParseLine("// C style comment"));
            Assert.IsNull(DatParser.ParseLine("/ Invalid comment"));
            Assert.IsNull(DatParser.ParseLine("# Comment"));

            Assert.AreEqual("192.168.1.1 - 192.168.1.254", DatParser.ParseLine("192.168.1.1 - 192.168.1.254 , 000 , Some organization"));
            Assert.AreEqual("192.168.1.1 - 192.168.1.254", DatParser.ParseLine("192.168.1.1 - 192.168.1.254 , 123 , Some organization"));

            // Access is > 127 so ignored
            Assert.IsNull(DatParser.ParseLine("192.168.1.1 - 192.168.1.254 , 128 , Some organization"));

            // Leading zeroes
            Assert.AreEqual("12.28.15.152 - 12.28.15.159", DatParser.ParseLine("012.028.015.152 - 012.028.015.159 , 000 , HILTON HOTEL CORPORATION"));
        }

        [TestMethod]
        public void ParseEntry()
        {
            var entry = DatParser.ParseEntry("001.002.003.001 - 001.002.003.254 , 000 , Description text");
            Assert.AreEqual(0x01030201, entry.From.Address);
            Assert.AreEqual(0xFE030201, entry.To.Address);
            Assert.AreEqual(0, entry.Level);
            Assert.AreEqual("Description text", entry.Description);
        }
    }
}
