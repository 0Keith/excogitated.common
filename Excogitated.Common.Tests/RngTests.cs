using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Excogitated.Common.Tests
{
    [TestClass]
    public class RngTests : TestsBase
    {
        private const int _totalIterations = 100000;
        private const double _delta = 0.1;
        private const int _minRange = 1;
        private const int _maxRange = 10;

        [TestMethod]
        public void Uniformity_Int32_PseudoRng()
        {
            var buckets = new Dictionary<int, AtomicInt32>();
            for (var i = _minRange; i <= _maxRange; i++)
                buckets[i] = new AtomicInt32();
            for (var i = 0; i < _totalIterations; i++)
                buckets[Rng.GetInt32(_minRange, _maxRange)].Increment();
            var expectedIterations = _totalIterations / _maxRange;
            var maxDelta = expectedIterations * _delta;
            for (var i = _minRange; i <= _maxRange; i++)
            {
                Console.WriteLine($"{i} : {buckets[i]}");
                Assert.AreEqual(expectedIterations, buckets[i], maxDelta, $"Bucket: {i}");
            }
        }

        [TestMethod]
        public void Uniformity_Int64_PseudoRng()
        {
            var buckets = new Dictionary<long, AtomicInt32>();
            for (var i = _minRange; i <= _maxRange; i++)
                buckets[i] = new AtomicInt32();
            for (var i = 0; i < _totalIterations; i++)
                buckets[Rng.GetInt64(_minRange, _maxRange)].Increment();
            var expectedIterations = _totalIterations / _maxRange;
            var maxDelta = expectedIterations * _delta;
            for (var i = _minRange; i <= _maxRange; i++)
            {
                Console.WriteLine($"{i} : {buckets[i]}");
                Assert.AreEqual(expectedIterations, buckets[i], maxDelta, $"Bucket: {i}");
            }
        }

        [TestMethod]
        public void GetText()
        {
            var characters = "abcdefghijklmnopqrstuvwxyz0123456789";
            var counts = new Dictionary<char, AtomicInt32>();
            foreach (var c in characters)
                counts[c] = new AtomicInt32();

            var text = Rng.GetText(_totalIterations, characters);
            foreach (var c in text)
                counts[c].Increment();

            var expectedCount = _totalIterations / characters.Length;
            var maxDelta = expectedCount * _delta;
            foreach (var c in characters)
            {
                var count = counts[c].Value;
                Console.WriteLine($"{c}:{count}");
                Assert.AreEqual(expectedCount, count, maxDelta, $"Character: {c}");
            }
        }

        [TestMethod]
        public void Uniformity_Double_PseudoRng()
        {
            var buckets = new Dictionary<int, AtomicInt32>();
            for (var i = _minRange; i <= _maxRange; i++)
                buckets[i] = new AtomicInt32();
            for (var i = 0; i < _totalIterations; i++)
            {
                var v = Rng.GetDouble(_minRange, _maxRange);
                var b = (int)v;
                buckets[b].Increment();
            }
            var expectedIterations = _totalIterations / _maxRange;
            var maxDelta = expectedIterations * _delta;
            for (var i = _minRange; i <= _maxRange; i++)
            {
                Console.WriteLine($"{i} : {buckets[i]}");
                Assert.AreEqual(expectedIterations, buckets[i], maxDelta, $"Bucket: {i}");
            }
        }
    }
}
