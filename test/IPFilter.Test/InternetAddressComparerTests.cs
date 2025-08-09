using IPFilter.Core;

namespace IPFilter.Test;

[TestClass]
public class InternetAddressComparerTests
{
    readonly InternetAddressComparer comparer = new();

    [TestMethod]
    public void Equal()
    {
        Assert.AreEqual(0, comparer.Compare(InternetAddress.Parse("192.168.1.254"), InternetAddress.Parse("192.168.1.254")));
    }

    [TestMethod]
    public void GreaterThan()
    {
        Assert.AreEqual(1, comparer.Compare(InternetAddress.Parse("192.168.1.254"), InternetAddress.Parse("192.168.1.253")));
        Assert.AreEqual(1, comparer.Compare(InternetAddress.Parse("6.0.0.1"), InternetAddress.Parse("6.0.0.0")));
        Assert.IsTrue(comparer.Compare(InternetAddress.Parse("6.0.0.0"), InternetAddress.Parse("3.255.255.255")) > 1);
    }

    [TestMethod]
    public void LessThan()
    {
        Assert.AreEqual(-1, comparer.Compare(InternetAddress.Parse("192.168.1.253"), InternetAddress.Parse("192.168.1.254")));
        Assert.IsTrue(comparer.Compare(InternetAddress.Parse("3.255.255.255"), InternetAddress.Parse("6.0.0.1")) < -1);
    }
}