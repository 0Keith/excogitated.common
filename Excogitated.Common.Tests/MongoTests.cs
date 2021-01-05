using Excogitated.Common.Mongo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Tests
{
    [TestClass]
    public class MongoTests
    {
        [TestMethod]
        public async Task AppConfigTest()
        {
            var db = new MongoStoreConfig
            {
                Server = "cluster0.y9pvv.mongodb.net",
                Database = "tests",
                Username = "admin",
                Password = "WlIjAmUwms8InmDI"
            }.GetDatabase();
            var settings = await db.GetAppSettings();
            var key = "test";
            var now = DateTimeOffset.Now;
            await settings.SetAsync(key, now);

            var value = await settings.GetAsync<DateTimeOffset>(key);
            Assert.AreEqual(now, value);

            var count = await settings.DeleteAsync(key);
            Assert.AreEqual(1, count);
        }
    }
}
