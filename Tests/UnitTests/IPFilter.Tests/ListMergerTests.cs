using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPFilter.Tests
{
    [TestClass]
    public class ListMergerTests
    {
        [TestMethod]
        public void Merge()
        {
            var lines1 = new[] { "000.000.000.000 - 000.255.255.255 , 000 , Bogon", "001.002.008.000 - 001.002.008.255 , 000 , China Internet Information Center (CNNIC)" };
            var lines2 = new[]
            {
                "001.002.004.000 - 001.002.004.255 , 000 , China Internet Information Center (CNNIC)",
                "001.009.102.251 - 001.009.102.251 , 000 , Botnet on Telekom Malaysia"
            };
            var lines3 = new[] { "001.009.096.105 - 001.009.096.105 , 000 , Botnet on Telekom Malaysia" };

            var results = new[]
            {
                "000.000.000.000 - 000.255.255.255 , 000 , Bogon",
                "001.002.004.000 - 001.002.004.255 , 000 , China Internet Information Center (CNNIC)",
                "001.002.008.000 - 001.002.008.255 , 000 , China Internet Information Center (CNNIC)",
                "001.009.096.105 - 001.009.096.105 , 000 , Botnet on Telekom Malaysia",
                "001.009.102.251 - 001.009.102.251 , 000 , Botnet on Telekom Malaysia"
            };
            
            var result = ListMerger.Merge(lines1, lines2, lines3).ToList();

            Assert.AreEqual("000.000.000.000 - 000.255.255.255 , 000 , Bogon", result[0]);
            Assert.AreEqual("001.002.004.000 - 001.002.004.255 , 000 , China Internet Information Center (CNNIC)", result[1]);
            Assert.AreEqual("001.002.008.000 - 001.002.008.255 , 000 , China Internet Information Center (CNNIC)", result[2]);
            Assert.AreEqual("001.009.096.105 - 001.009.096.105 , 000 , Botnet on Telekom Malaysia", result[3]);
            Assert.AreEqual("001.009.102.251 - 001.009.102.251 , 000 , Botnet on Telekom Malaysia", result[4]);
        }
    }

    public class ListMerger
    {
        public static IEnumerable<string> Merge(params IEnumerable<string>[] lines)
        {
            return lines.SelectMany(x => x).OrderBy(x => x);
        }
    }
}
