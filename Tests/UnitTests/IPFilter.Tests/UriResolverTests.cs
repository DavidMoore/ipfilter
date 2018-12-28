using System.Threading.Tasks;
using IPFilter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace IPFilter.Tests
{
    [TestClass]
    public class UriResolverTests
    {
        IUriResolver resolver = new UriResolver();

        [TestMethod]
        public async Task Relative_paths_not_supported()
        {
            Assert.IsNull(resolver.Resolve("/test/test.dat"));
            Assert.IsNull(resolver.Resolve(@"\test/test.dat"));
        }

        [TestMethod]
        public void LocalPath()
        {
            var uri = resolver.Resolve(@"C:\TEMP\subfolder\..\\test.dat");
            Assert.IsTrue(uri.IsFile);
            Assert.IsFalse(uri.IsUnc);
            Assert.AreEqual(@"C:\TEMP\test.dat", uri.LocalPath);
            Assert.AreEqual("file:///C:/TEMP/test.dat", uri.ToString());
        }

        [TestMethod]
        public void UNC()
        {
            var uri = resolver.Resolve(@"\\HOSTNAME\share\subfolder\..\test.dat");
            Assert.IsTrue(uri.IsUnc);
            Assert.AreEqual("file://hostname/share/test.dat", uri.ToString());
        }

        [TestMethod]
        public void Unsupported_schemes()
        {
            Assert.IsNull(resolver.Resolve("ftp://host/file.dat"));
        }
    }
}
