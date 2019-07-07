using System.IO;
using System.Runtime.Remoting;
using IPFilter.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPFilter.Tests.Services
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void Save()
        {
            var config = Config.Default;

            Config.Save(config, "test.json");

            config = Config.Parse(File.ReadAllText("test.json"));

            Assert.AreEqual(false, config.settings.cache.isEnabled);
            Assert.AreEqual(false, config.settings.task.isEnabled);
            Assert.AreEqual(false, config.settings.update.isPreReleaseEnabled);
            Assert.AreEqual(false, config.settings.update.isDisabled);
        }

        [TestMethod]
        public void Reload()
        {
            var config = Config.Default;

            config.settings.task.isEnabled = true;
            config.settings.update.isPreReleaseEnabled = true;
            config.settings.update.isDisabled = true;
            
            Config.Reload();

            config = Config.Default;

            Assert.AreEqual(false, config.settings.cache.isEnabled);
            Assert.AreEqual(false, config.settings.task.isEnabled);
            Assert.AreEqual(false, config.settings.update.isPreReleaseEnabled);
            Assert.AreEqual(false, config.settings.update.isDisabled);
        }

        [TestMethod]
        public void Parse()
        {
            var json = @"{
    ""settings"": {
      ""update"": {
        ""isDisabled"": false,
        ""isPreReleaseEnabled"": false
      },
      ""task"": {
        ""isEnabled"": false
      },
      ""cache"": {
        ""isEnabled"": false
      }
    }
}";

            var config = Config.Parse(json);

            Assert.IsNotNull(config);
            Assert.AreEqual(false, config.settings.cache.isEnabled);
            Assert.AreEqual(false, config.settings.task.isEnabled);
            Assert.AreEqual(false, config.settings.update.isPreReleaseEnabled);
            Assert.AreEqual(false, config.settings.update.isDisabled);

            config = Config.Parse(@"{
    ""settings"": {
      ""update"": {
        ""isDisabled"": true,
        ""isPreReleaseEnabled"": true
      },
      ""task"": {
        ""isEnabled"": true
      },
      ""cache"": {
        ""isEnabled"": true
      }
    }
}");
            Assert.AreEqual(true, config.settings.cache.isEnabled);
            Assert.AreEqual(true, config.settings.task.isEnabled);
            Assert.AreEqual(true, config.settings.update.isPreReleaseEnabled);
            Assert.AreEqual(true, config.settings.update.isDisabled);
        }

        [TestMethod]
        public void Default()
        {
            Assert.AreEqual(false, Config.Default.settings.task.isEnabled);
            Assert.AreEqual(false, Config.Default.settings.cache.isEnabled);
            Assert.AreEqual(false, Config.Default.settings.update.isPreReleaseEnabled);
            Assert.AreEqual(false, Config.Default.settings.update.isDisabled);
        }

        [TestMethod]
        public void DefaultFallback()
        {
            var config = new Config.Configuration();

            Assert.AreEqual(false, config.settings.task.isEnabled);
            Assert.AreEqual(false, config.settings.cache.isEnabled);
            Assert.AreEqual(false, config.settings.update.isPreReleaseEnabled);
            Assert.AreEqual(false, config.settings.update.isDisabled);
            Assert.AreEqual(0, config.outputs.Count);
        }
    }
}
