using System.Globalization;
using IPFilter.Native;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPFilter.Tests
{
    [TestClass]
    public class PathHelperTests
    {
        [TestMethod]
        public void JapanesePathShouldConvertYenToBackslash()
        {
            var path = "C:¥Program Files¥qBittorrent¥uninst.exe";
            var normalized = PathHelper.GetDirectoryInfo(path, CultureInfo.GetCultureInfo("ja-JP"));
            Assert.AreEqual("C:\\Program Files\\qBittorrent\\uninst.exe", normalized.FullName);
        }
    }
}
