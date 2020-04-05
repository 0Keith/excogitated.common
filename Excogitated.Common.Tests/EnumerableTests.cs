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

        [TestMethod]
        public void MinMaxSelect()
        {
            var values = Enumerable.Range(1, 10000).Randomize();
            var min = values.MinSelect(i => i.ToDecimal());
            var max = values.MaxSelect(i => i.ToDecimal());
            Assert.AreEqual(1, min);
            Assert.AreEqual(10000, max);
        }

        [TestMethod]
        public void SelectSplit()
        {
            using var batches = Enumerable.Range(1, 123).SelectSplit(e => e.Take(50)).GetEnumerator();
            Assert.IsTrue(batches.MoveNext());
            var values = batches.Current.ToList();
            Assert.AreEqual(values.Count, 50);
            Assert.AreEqual(values[0], 1);
            Assert.AreEqual(values[49], 50);

            Assert.IsTrue(batches.MoveNext());
            values = batches.Current.ToList();
            Assert.AreEqual(values.Count, 50);
            Assert.AreEqual(values[0], 51);
            Assert.AreEqual(values[49], 100);

            Assert.IsTrue(batches.MoveNext());
            values = batches.Current.ToList();
            Assert.AreEqual(values.Count, 23);
            Assert.AreEqual(values[0], 101);
            Assert.AreEqual(values[22], 123);
        }

        [TestMethod]
        public void Flatten()
        {
            var items = Enumerable.Range(0, 1000).Select(i => Enumerable.Range(i, 1000)).Flatten();
            Assert.AreEqual(1000 * 1000, items.Count());
        }
    }
}
