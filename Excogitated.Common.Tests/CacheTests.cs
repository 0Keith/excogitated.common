using Excogitated.Common.Caching;
using Excogitated.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common.Tests
{
    internal class TestDataProvider : ICacheDataProvider<int, string>
    {
        public int RefreshCount { get; private set; }
        public int SearchCount { get; private set; }

        public async Task<IEnumerable<string>> GetData()
        {
            RefreshCount++;
            await Task.Delay(1000);
            return Enumerable.Range(0, 10000).Select(i => i.ToString());
        }

        public bool SearchData(string keyword, int key, string value)
        {
            SearchCount++;
            return value.Contains(keyword) || keyword.Contains(value);
        }

        public void Log(Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
        }

        public int GetKey(string value)
        {
            return int.Parse(value);
        }
    }

    [TestClass]
    public class CacheTests : TestsBase
    {
        [TestMethod]
        public async Task GetAsync_NoRefresh()
        {
            var cache = new JsonFileMemoryCache<int, string>(new JsonFileMemoryCacheSettings());
            var dataProvider = new TestDataProvider();
            foreach (var i in Enumerable.Range(0, 10000).OrderBy(i => Rng.GetInt32()))
            {
                var value = await cache.GetAsync(i, dataProvider);
                Assert.AreEqual(i.ToString(), value);
            }
            await Task.Delay(200);
            foreach (var i in Enumerable.Range(10000, 10000).OrderBy(i => Rng.GetInt32()))
            {
                var value = await cache.GetAsync(i, dataProvider);
                Assert.IsNull(value);
            }
            Assert.AreEqual(1, dataProvider.RefreshCount);
        }

        [TestMethod]
        public async Task GetAsync_OneSecondRefresh()
        {
            var cache = new JsonFileMemoryCache<int, string>(new JsonFileMemoryCacheSettings
            {
                RefreshInterval = TimeSpan.FromMilliseconds(100),
            });
            var dataProvider = new TestDataProvider();
            foreach (var i in Enumerable.Range(0, 10000).OrderBy(i => Rng.GetInt32()))
            {
                var value = await cache.GetAsync(i, dataProvider);
                Assert.AreEqual(i.ToString(), value);
            }
            await Task.Delay(200);
            foreach (var i in Enumerable.Range(10000, 10000).OrderBy(i => Rng.GetInt32()))
            {
                var value = await cache.GetAsync(i, dataProvider);
                Assert.IsNull(value);
            }
            Assert.AreEqual(2, dataProvider.RefreshCount);
        }

        [TestMethod]
        public async Task SearchAsync()
        {
            var cache = new JsonFileMemoryCache<int, string>(new JsonFileMemoryCacheSettings
            {
                RefreshInterval = TimeSpan.FromMilliseconds(100),
            });
            var dataProvider = new TestDataProvider();
            var expected = await cache.SearchAsync("0", dataProvider);
            await Task.Delay(200);
            var actual = await cache.SearchAsync("0", dataProvider);
            Assert.IsTrue(expected.SequenceEqual(actual));
            Assert.AreEqual(20000, dataProvider.SearchCount);
            Assert.AreEqual(2, dataProvider.RefreshCount);
        }

        [TestMethod]
        public async Task GetAsync_From_FilePath()
        {
            var settings = new JsonFileMemoryCacheSettings
            {
                RefreshInterval = TimeSpan.FromSeconds(5),
                FilePath = "./test.json",
            };
            await new FileInfo(settings.FilePath + ".zip").DeleteAsync();

            var cache = new JsonFileMemoryCache<int, string>(settings);
            var dataProvider = new TestDataProvider();
            foreach (var i in Enumerable.Range(0, 10000).OrderBy(i => Rng.GetInt32()))
            {
                var value = await cache.GetAsync(i, dataProvider);
                Assert.AreEqual(i.ToString(), value);
            }
            cache = new JsonFileMemoryCache<int, string>(settings);
            foreach (var i in Enumerable.Range(10000, 10000).OrderBy(i => Rng.GetInt32()))
            {
                var value = await cache.GetAsync(i, dataProvider);
                Assert.IsNull(value);
            }
            Assert.AreEqual(1, dataProvider.RefreshCount);
        }


        [TestMethod]
        public async Task SearchAsync_From_FilePath()
        {
            var settings = new JsonFileMemoryCacheSettings
            {
                RefreshInterval = TimeSpan.FromSeconds(5),
                FilePath = "./test.json",
            };
            await new FileInfo(settings.FilePath + ".zip").DeleteAsync();

            var cache = new JsonFileMemoryCache<int, string>(settings);
            var dataProvider = new TestDataProvider();
            var expected = await cache.SearchAsync("0", dataProvider);
            var actual = await cache.SearchAsync("0", dataProvider);
            Assert.IsTrue(expected.SequenceEqual(actual));
            Assert.AreEqual(10000, dataProvider.SearchCount);
            Assert.AreEqual(1, dataProvider.RefreshCount);

            cache = new JsonFileMemoryCache<int, string>(settings);
            actual = await cache.SearchAsync("0", dataProvider);
            Assert.IsTrue(expected.SequenceEqual(actual));
            Assert.AreEqual(20000, dataProvider.SearchCount);
            Assert.AreEqual(1, dataProvider.RefreshCount);

            actual = await cache.SearchAsync("0", dataProvider);
            Assert.IsTrue(expected.SequenceEqual(actual));
            Assert.AreEqual(20000, dataProvider.SearchCount);
            Assert.AreEqual(1, dataProvider.RefreshCount);
        }
    }
}