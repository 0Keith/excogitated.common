using Excogitated.Common.Mongo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Tests
{
    internal class TestSettingDocument
    {
        public decimal Test0 { get; set; }
        public string Test1 { get; set; }
        public DateTimeOffset Test2 { get; set; }
        public Date Test3 { get; set; }
    }

    [TestClass]
    public class MongoTests
    {
        private readonly IMongoDatabase _db = new MongoStoreConfig
        {
            Server = "cluster0.y9pvv.mongodb.net",
            Database = "tests",
            Username = "admin",
            Password = "WlIjAmUwms8InmDI"
        }.GetDatabase();

        [TestMethod]
        public async Task AppConfigTest()
        {
            var settings = await _db.GetAppSettings<TestSettingDocument>();
            var expected0 = Rng.GetDecimal();
            await settings.SetAsync(d => d.Test0, expected0);
            var value0 = await settings.GetAsync(d => d.Test0);
            Assert.AreEqual(expected0, value0);

            var expected1 = Rng.GetText(1000);
            await settings.SetAsync(d => d.Test1, expected1);
            var value1 = await settings.GetAsync(d => d.Test1);
            Assert.AreEqual(expected1, value1);

            var expected2 = DateTimeOffset.Now;
            await settings.SetAsync(d => d.Test2, expected2);
            var value2 = await settings.GetAsync(d => d.Test2);
            Assert.AreEqual(expected2, value2);

            var expected3 = Date.Today;
            await settings.SetAsync(d => d.Test3, expected3);
            var value3 = await settings.GetAsync(d => d.Test3);
            Assert.AreEqual(expected3, value3);
        }
    }
}
