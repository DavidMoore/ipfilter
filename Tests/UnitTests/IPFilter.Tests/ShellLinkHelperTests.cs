using System;
using IPFilter.Native;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPFilter.Tests
{
    [TestClass]
    public class ShellLinkHelperTests
    {
        [TestMethod]
        public void Shortcuts()
        {
            var path = ShellLinkHelper.ResolveShortcut(@"C:\Users\Default\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Accessibility\Magnify.lnk");
            var expectedPath = Environment.ExpandEnvironmentVariables(@"%windir%\system32\magnify.exe");
            Assert.AreEqual(expectedPath, path);
        }
    }
}
