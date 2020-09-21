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
        public async ValueTask<string> GetValue(int key, CacheResult<string> result)
        {
            RefreshCount++;
            return key.ToString();
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
                Assert.AreEqual(CacheSource.Factory, result.Source);
            }
            Assert.AreEqual(10000, dataProvider.RefreshCount);

            await Task.Delay(200);
            foreach (var i in items)
            {
                var result = await cache.GetAsync(i, dataProvider);
                Assert.AreEqual(i.ToString(), result.Value);
                Assert.AreEqual(CacheSource.Cache, result.Source);
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
                Assert.AreEqual(CacheSource.Factory, result.Source);
            }
            Assert.AreEqual(10000, dataProvider.RefreshCount);

            await Task.Delay(200);
            foreach (var i in items)
            {
                var result = await cache.GetAsync(i, dataProvider);
                Assert.AreEqual(i.ToString(), result.Value);
                Assert.AreEqual(CacheSource.Cache, result.Source);
            }
            Assert.AreEqual(10000, dataProvider.RefreshCount);

            await Task.Delay(5000);
            foreach (var i in items)
            {
                var result = await cache.GetAsync(i, dataProvider);
                Assert.AreEqual(i.ToString(), result.Value);
                Assert.AreEqual(CacheSource.Factory, result.Source);
            }
            Assert.AreEqual(20000, dataProvider.RefreshCount);
        }

        [TestMethod]
        public async Task GetAsync_BackupFactory()
        {
            var cache = new CowCache(new CowCacheSettings
            {
                RefreshInterval = TimeSpan.FromMilliseconds(5000),
            });
            var result = await cache.GetAsync<int, string>(0, (k, p) => throw new Exception(k.ToString()), async (k, p) => k.ToString());
            Assert.AreEqual("0", result.Value);
            Assert.AreEqual(CacheSource.BackupFactory, result.Source);
            Assert.AreEqual("0", result.Exception?.Message);
        }
    }
}