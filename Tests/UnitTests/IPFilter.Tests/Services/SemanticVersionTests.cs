using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPFilter.Services.Deployment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPFilter.Tests.Services
{
    [TestClass]
    public class SemanticVersionTests
    {
        [TestMethod]
        public void Comparison()
        {
            var current = new SemanticVersion("3.0.0-beta1");
            var newVersion = new SemanticVersion("3.0.1.4-beta");
            Assert.IsTrue(newVersion > current);
        }
    }
}
