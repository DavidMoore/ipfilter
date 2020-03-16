using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Script.Serialization;
namespace IPFilter.Tests.Apps
{
    [TestClass]
    public class DelugeTests
    {
        internal static readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        [TestMethod, Ignore]
        public void ParseConfig()
        {
            var conf = "{\n  \"file\": 1, \n  \"format\": 1\n}{\n  \"check_after_days\": 1, \n  \"timeout\": 180, \n  \"url\": \"https://github.com/DavidMoore/ipfilter/releases/download/lists/ipfilter.dat.gz\", \n  \"try_times\": 3, \n  \"list_size\": 3053919, \n  \"last_update\": 1583488027.858, \n  \"list_type\": \"\", \n  \"list_compression\": \"\", \n  \"load_on_start\": false\n}";


            var obj = serializer.DeserializeObject(conf);
            
            Assert.IsNotNull(obj);

        }

        class DelugeSerializer
        {

        }
    }
}
