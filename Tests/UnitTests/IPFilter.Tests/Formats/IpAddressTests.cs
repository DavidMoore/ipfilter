using IPFilter.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPFilter.Tests.Formats
{
    [TestClass]
    public class IpAddressTests
    {
        [TestMethod]
        public void ParseAddress()
        {
            Assert.AreEqual(0xC0A801FE, IpAddress.Parse("192.168.1.254"));
        }

        [TestMethod]
        public void GetBytes()
        {
            var bytes = IpAddress.GetBytes(0x010203FE);
            Assert.AreEqual(0x01, bytes[0]);
            Assert.AreEqual(0x02, bytes[1]);
            Assert.AreEqual(0x03, bytes[2]);
            Assert.AreEqual(254, bytes[3]);
        }

        [TestMethod]
        public void ReverseBytes()
        {
            Assert.AreEqual(0x04030201, IpAddress.ReverseBytes(0x01020304) );
            Assert.AreEqual((uint)0x04030201, IpAddress.ReverseBytes((uint)0x01020304) );
        }
    }
}