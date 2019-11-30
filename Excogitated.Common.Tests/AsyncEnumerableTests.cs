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
        public void ReverseFast()
        {
            var items = Enumerable.Range(-1000, 2000).ToList();
            var reversed = items.ReverseFast().ToList();
            Assert.AreEqual(items.Count, reversed.Count);
            Assert.IsTrue(items.SequenceEqual(reversed.ReverseFast()));
        }

        [TestMethod]
        public async Task Batch()
        {
            var items = Enumerable.Range(-1000, 2000).ToList();
            var threadCount = Environment.ProcessorCount / 2;
            var ids = await items.ToAsync().Batch(threadCount, async i => Thread.GetCurrentProcessorId()).ToList();
            Assert.AreEqual(items.Count, ids.Count);
            Assert.AreEqual(threadCount, ids.Distinct().Count());
        }
    }
}
