namespace IPFilter.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Linq;
    using Services;

    [TestClass]
    public class DestinationPathsProviderTests
    {
        DestinationPathsProvider destinationPathProvider;

        [TestInitialize]
        public void Initialize()
        {
            destinationPathProvider = new DestinationPathsProvider();   
        }

        [TestMethod]
        public void Expands_environment_variables()
        {
            var results = destinationPathProvider.GetDestinations(new[] { @"%TEMP%"});
            Assert.AreEqual(Environment.ExpandEnvironmentVariables("%TEMP%"), results.Single());
        }

        [TestMethod]
        public void Strips_duplicates()
        {
            var results = destinationPathProvider.GetDestinations(new[] {@"%TEMP%", @"%TEMP%", @"%APPDATA%"}).ToList();

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(Environment.ExpandEnvironmentVariables("%TEMP%"), results[0]);
            Assert.AreEqual(Environment.ExpandEnvironmentVariables("%APPDATA%"), results[1]);
        }


        [TestMethod]
        public void Case_insensitive_duplicates()
        {
            var results = destinationPathProvider.GetDestinations(new[] { @"testing", @"Testing", @"tEstiNg" });
            Assert.AreEqual(Environment.ExpandEnvironmentVariables("testing"), results.Single());
        }

        [TestMethod]
        public void Special_character_duplicates()
        {
            var results = destinationPathProvider.GetDestinations(new[] { @"testing\\", @"testing"}).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(@"testing", results[0]);
        }
        
        [TestMethod]
        public void Whitespace_duplicates()
        {
            var results = destinationPathProvider.GetDestinations(new[] { @" testing\\ \\", @"testing", @"   testing" }).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(@"testing", results[0]);
        }

        [TestMethod]
        public void Null_or_empty_arguments_return_empty_array()
        {
            Assert.AreEqual(0, destinationPathProvider.GetDestinations(null).Count());
            Assert.AreEqual(0, destinationPathProvider.GetDestinations(new string[]{}).Count());
        }
    }
}