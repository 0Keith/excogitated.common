using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Excogitated.Common.Test
{
    [TestClass]
    public class EnumerableTests
    {
        [TestMethod]
        public void ReverseFast()
        {
            var items = Enumerable.Range(-1000, 2000).ToList();
            var reversed = items.ReverseFast().ToList();
            Assert.AreEqual(items.Count, reversed.Count);
            Assert.IsTrue(items.SequenceEqual(reversed.ReverseFast()));
        }

        [TestMethod]
        public void SkipLast()
        {
            var items = Enumerable.Range(-1000, 2000).ToList();
            var count = items.SkipLast(10).Count();
            Assert.AreEqual(items.Count - 10, count);
        }

        [TestMethod]
        public void Aggregate()
        {
            var value = Enumerable.Range(1, 4).Aggregate(10, (i1, i2, s) => i1 * i2 + s);
            Assert.AreEqual(30, value);
        }
    }
}
