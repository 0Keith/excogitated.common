using Excogitated.Common.Caching;
using Excogitated.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common.Tests
{
    internal class TestDataFactory : ICacheValueFactory<int, string>
    {
        public int RefreshCount { get; private set; }
        public Task<string> GetValue(int key, CacheResult<string> result)
        {
            RefreshCount++;
            return Task.FromResult(key.ToString());
        }
    }

    [TestClass]
    public class CacheTests : TestsBase
    {
        [TestMethod]
        public async Task GetAsync_NoRefresh()
        {
            var cache = new CowCache(new CowCacheSettings());
            var dataProvider = new TestDataFactory();
            var items = Enumerable.Range(0, 10000).OrderBy(i => Rng.GetInt32());
            foreach (var i in items)
            {
                var result = await cache.GetAsync(i, dataProvider);
                Assert.AreEqual(i.ToString(), result.Value);
                Assert.IsTrue(result.FromFactory);
            }
            Assert.AreEqual(10000, dataProvider.RefreshCount);

            await Task.Delay(200);
            foreach (var i in items)
            {
                var result = await cache.GetAsync(i, dataProvider);
                Assert.AreEqual(i.ToString(), result.Value);
                Assert.IsTrue(result.FromCache);
            }
            Assert.AreEqual(10000, dataProvider.RefreshCount);
        }

        [TestMethod]
        public async Task GetAsync_OneSecondRefresh()
        {
            var cache = new CowCache(new CowCacheSettings
            {
                RefreshInterval = TimeSpan.FromMilliseconds(5000),
            });
            var dataProvider = new TestDataFactory();
            var items = Enumerable.Range(0, 10000).OrderBy(i => Rng.GetInt32());
            foreach (var i in items)
            {
                var result = await cache.GetAsync(i, dataProvider);
                Assert.AreEqual(i.ToString(), result.Value);
                Assert.IsTrue(result.FromFactory, result.ToString());
            }
            Assert.AreEqual(10000, dataProvider.RefreshCount);

            await Task.Delay(200);
            foreach (var i in items)
            {
                var result = await cache.GetAsync(i, dataProvider);
                Assert.AreEqual(i.ToString(), result.Value);
                Assert.IsTrue(result.FromCache, result.ToString());
            }
            Assert.AreEqual(10000, dataProvider.RefreshCount);

            await Task.Delay(5000);
            foreach (var i in items)
            {
                var result = await cache.GetAsync(i, dataProvider);
                Assert.AreEqual(i.ToString(), result.Value);
                Assert.IsTrue(result.FromFactory, result.ToString());
            }
            Assert.AreEqual(20000, dataProvider.RefreshCount);
        }
    }
}