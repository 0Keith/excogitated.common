using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.Common.Test
{
    [TestClass]
    public class AsyncEnumerableTests
    {
        [TestMethod]
        public async Task Take_Count()
        {
            var count = await Enumerable.Range(0, 100).ToAsync().Take(77).Count();
            Assert.AreEqual(77, count);
        }

        [TestMethod]
        public async Task Skip_Count()
        {
            var count = await Enumerable.Range(0, 100).ToAsync().Skip(77).Count();
            Assert.AreEqual(23, count);
        }

        [TestMethod]
        public async Task Min()
        {
            var min = await Enumerable.Range(-100, 200).ToAsync().Min();
            Assert.AreEqual(-100, min);
        }

        [TestMethod]
        public async Task Max()
        {
            var max = await Enumerable.Range(1, 100).ToAsync().Max();
            Assert.AreEqual(100, max);
        }

        [TestMethod]
        public async Task Average()
        {
            var avg = await Enumerable.Range(-100, 201).ToAsync().Average(i => i);
            Assert.AreEqual(0, avg);
        }

        [TestMethod]
        public async Task Batch()
        {
            var items = Enumerable.Range(-1000, 1000).ToList();
            var threadCount = 2;
            var ids = await items.ToAsync().Batch(threadCount, async i =>
            {
                await AsyncTimer.Delay(1);
                return Thread.CurrentThread.ManagedThreadId;
            }).ToList();
            foreach (var id in ids.Distinct())
                Console.WriteLine(id);
            Assert.AreEqual(items.Count, ids.Count);
            Assert.IsTrue(threadCount <= ids.Distinct().Count());
        }

        [TestMethod]
        public async Task Aggregate()
        {
            var value = await Enumerable.Range(1, 4).ToAsync().Aggregate(10, (i1, i2, s) => i1 * i2 + s);
            Assert.AreEqual(30, value);
        }

        [TestMethod]
        public async Task MinMaxSelect()
        {
            var values = Enumerable.Range(1, 10000).Randomize().ToAsync();
            var min = await values.MinSelect(i => i.ToDecimal());
            var max = await values.MaxSelect(i => i.ToDecimal());
            Assert.AreEqual(1, min);
            Assert.AreEqual(10000, max);
        }
    }
}
