using System;
using System.Net;
using IPFilter.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPFilter.Tests
{
    [TestClass]
    public class AddressTests
    {
        [TestMethod]
        public void Ctor()
        {
            var ipAddress = IPAddress.Parse("192.168.1.123");

            var address = new Address( IpAddress.Parse("192.168.1.123") );

            Assert.AreEqual( (byte)192, address.OctetA);
            Assert.AreEqual( (byte)168, address.OctetB);
            Assert.AreEqual( (byte)1, address.OctetC);
            Assert.AreEqual( (byte)123, address.OctetD);

            address = new Address((uint)IpAddress.ReverseBytes((int)ipAddress.Address));

            Assert.AreEqual((byte)192, address.OctetA);
            Assert.AreEqual((byte)168, address.OctetB);
            Assert.AreEqual((byte)1, address.OctetC);
            Assert.AreEqual((byte)123, address.OctetD);
        }

        [TestMethod]
        public void IsLoopback()
        {
            // TODO: Test range
            var address = new Address(IpAddress.Parse("127.0.0.1") );
            Assert.IsTrue(address.IsLoopback);
            Assert.IsFalse(address.IsPrivate);
            Assert.IsFalse(address.IsReserved);
        }

        [TestMethod]
        public void IsPrivate()
        {
            // TODO: Test range
            var address = new Address(IpAddress.Parse("192.168.0.1") );
            Assert.IsFalse(address.IsLoopback);
            Assert.IsTrue(address.IsPrivate);
            Assert.IsFalse(address.IsReserved);
        }

        [TestMethod]
        public void IsReserved()
        {
            // TODO: Test range
            var address = new Address(IpAddress.Parse("0.0.0.1") );
            Assert.IsFalse(address.IsLoopback);
            Assert.IsFalse(address.IsPrivate);
            Assert.IsTrue(address.IsReserved);
        }
    }
}
