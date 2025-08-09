using IPFilter.Core;

namespace IPFilter.Test;

[TestClass]
public sealed class InternetAddressTests
{
    [TestMethod]
    public void SpanTest()
    {
        Assert.AreEqual(0xC0A801FE, InternetAddress.Parse("192.168.1.254"));
    }
}