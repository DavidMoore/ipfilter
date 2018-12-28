using System.Collections.Generic;
using System.Net;
using IPFilter.Core;
using IPFilter.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPFilter.Tests.Formats
{
    [TestClass]
    public class IPAddressComparerTests
    {
        readonly IPAddressComparer comparer = new IPAddressComparer();

        [TestMethod]
        public void Equal()
        {
            Assert.AreEqual(0, comparer.Compare( DatParser.ParseAddress("192.168.1.254"), DatParser.ParseAddress("192.168.1.254") ) );
        }

        [TestMethod]
        public void GreaterThan()
        {
            Assert.AreEqual(1, comparer.Compare(DatParser.ParseAddress("192.168.1.254"), DatParser.ParseAddress("192.168.1.253")));
            Assert.AreEqual(1, comparer.Compare(DatParser.ParseAddress("6.0.0.1"), DatParser.ParseAddress("6.0.0.0")));
            Assert.IsTrue(comparer.Compare(DatParser.ParseAddress("6.0.0.0"), DatParser.ParseAddress("3.255.255.255")) > 1);
        }

        [TestMethod]
        public void LessThan()
        {
            Assert.AreEqual(-1, comparer.Compare(DatParser.ParseAddress("192.168.1.253"), DatParser.ParseAddress("192.168.1.254")));
            Assert.IsTrue( comparer.Compare(DatParser.ParseAddress("3.255.255.255"), DatParser.ParseAddress("6.0.0.1")) < -1);
        }
    }
}