using IPFilter.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPFilter.Tests.Formats
{
    [TestClass]
    public class FilterEntryComparerTests
    {

        [TestMethod]
        public void Equal()
        {
            Assert.AreEqual(0, FilterEntry.Comparer.Compare( new FilterEntry("192.168.1.1", "192.168.1.254"), new FilterEntry("192.168.1.1", "192.168.1.254")));
        }

        [TestMethod]
        public void Null()
        {
            Assert.AreEqual(-1, FilterEntry.Comparer.Compare(null, new FilterEntry("192.168.1.1", "192.168.1.254")));
            Assert.AreEqual(0, FilterEntry.Comparer.Compare(null, null));
            Assert.AreEqual(1, FilterEntry.Comparer.Compare(new FilterEntry("192.168.1.1", "192.168.1.254"),null));
        }

        [TestMethod]
        public void LessThan()
        {
            // x and y start at the same address, but x has a smaller range
            Assert.AreEqual(-1, FilterEntry.Comparer.Compare(new FilterEntry("192.168.1.1", "192.168.1.253"), new FilterEntry("192.168.1.1", "192.168.1.254")));

            // x starts before y
            Assert.AreEqual(-1, FilterEntry.Comparer.Compare(new FilterEntry("192.168.1.1", "192.168.1.253"), new FilterEntry("192.168.1.2", "192.168.1.2")));
        }

        [TestMethod]
        public void GreaterThan()
        {
            // x starts after y, even though y has a big range.
            Assert.AreEqual(1, FilterEntry.Comparer.Compare(new FilterEntry("192.168.1.2", "192.168.1.2"), new FilterEntry("192.168.1.1", "192.168.1.254")));

            // They start at the same address, but x has a higher range
            Assert.IsTrue(FilterEntry.Comparer.Compare(new FilterEntry("192.168.1.2", "192.168.1.253"), new FilterEntry("192.168.1.2", "192.168.1.250")) > 0);
        }
    }
}