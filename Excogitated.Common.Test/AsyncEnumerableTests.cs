using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
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
            var count = await Enumerable.Range(-100, 100).ToAsync().Min();
            Assert.AreEqual(-100, count);
        }

        [TestMethod]
        public async Task Max()
        {
            var count = await Enumerable.Range(1, 100).ToAsync().Max();
            Assert.AreEqual(100, count);
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
            var items = Enumerable.Range(-1000, 1000).ToList();
            var reversed = items.ReverseFast().ToList();
            Assert.AreEqual(items.Count, reversed.Count);
            Assert.IsTrue(items.SequenceEqual(reversed.ReverseFast()));
        }
    }
}
