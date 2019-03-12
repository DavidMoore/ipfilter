using System.Collections.Generic;
using System.Linq;
using IPFilter.Core;
using IPFilter.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPFilter.Tests.Formats
{
    [TestClass]
    public class FilterCollectionTests
    {
        [TestMethod]
        public void Merge()
        {
            var original = new List<FilterEntry>
            {
                new FilterEntry("192.168.1.7", "192.168.1.253"),
                new FilterEntry("6.0.0.1", "6.255.255.254"),
                new FilterEntry("192.168.1.1", "192.168.1.200"),
                new FilterEntry("192.168.1.2", "192.168.1.2"),
                new FilterEntry("192.168.1.2", "192.168.1.20"),
                new FilterEntry("192.168.1.254", "192.168.1.254"),
                new FilterEntry("3.0.0.0", "3.255.255.255"),
                new FilterEntry("4.0.0.0", "4.0.0.1"),
                new FilterEntry("6.0.0.0", "6.255.255.255"),
                new FilterEntry("192.168.1.2", "192.168.1.2")
            };

            var result = FilterCollection.Merge(original);

            Assert.AreEqual(3, result.Count);

            Assert.AreEqual( new FilterEntry("3.0.0.0", "4.0.0.1"), result[0] );

            Assert.AreEqual( new FilterEntry("6.0.0.0", "6.255.255.255"), result[1] );
            Assert.AreEqual( new FilterEntry("192.168.1.1", "192.168.1.254"), result[2] );
        }

        [TestMethod]
        public void Sort()
        {
            var original = new List<FilterEntry>
            {
                new FilterEntry("192.168.1.7", "192.168.1.253"),
                new FilterEntry("6.0.0.1", "6.255.255.254"),
                new FilterEntry("3.0.0.0", "3.255.255.255")
            };

            var result = FilterCollection.Sort(original);

            Assert.AreEqual(3, result.Count);

            Assert.AreEqual(new FilterEntry("3.0.0.0", "3.255.255.255"), result[0]);
            Assert.AreEqual(new FilterEntry("6.0.0.1", "6.255.255.254"), result[1]);
            Assert.AreEqual(new FilterEntry("192.168.1.7", "192.168.1.253"), result[2]);
        }
    }
}
