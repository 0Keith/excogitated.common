using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common.Tests
{
    [TestClass]
    public class StringTests : TestsBase
    {

        [TestMethod]
        public async Task LowerVsUpperHashcodeTest()
        {
            var msg = string.Empty;
            var lowerChars = new string(Enumerable.Range('a', 26).Select(i => (char)i).ToArray());
            var upperChars = new string(Enumerable.Range('A', 26).Select(i => (char)i).ToArray());

            var max = 1000000L;
            var count = new AtomicInt64();
            await Enumerable.Range(0, Environment.ProcessorCount / 2).Select(n => Task.Run(() =>
            {
                var i = count.Increment();
                while (i <= max)
                {
                    var lower = i.ToBigInt().ToBase(lowerChars);
                    var upper = i.ToBigInt().ToBase(upperChars);
                    var e = msg = $"Lower: {lower}, Upper: {upper}";
                    Assert.AreNotEqual(lower.GetHashCode(), upper.GetHashCode(), e);
                    i = count.Increment();
                }
            })).WhenAll();
            Console.WriteLine(msg);
        }

        [TestMethod]
        public void BaseCoversionTest()
        {
            var value = Rng.GetInt32();
            if (value < 0)
                value *= -1;

            var digits = "0123456789abcdef";
            var expected = Convert.ToString(value, 16);
            var actual = value.ToBigInt().ToBase(digits);
            Assert.AreEqual(expected, actual);

            var base10 = actual.ToBase10(digits);
            Assert.AreEqual(value, base10);
        }

        [TestMethod]
        public void DiffTest()
        {
            var item = Jsonizer.DeepCopy(JsonTests.Item);
            var expected = Jsonizer.Serialize(item);
            item.ListDate = Date.Today.AddDays(17);
            var actual = Jsonizer.Serialize(item);
            var diff = expected.Diff(actual);
            Console.WriteLine(diff);
            Assert.IsTrue(diff.DifferenceFound);
            Assert.IsTrue(diff.Expected.IsNotNullOrWhiteSpace());
            Assert.IsTrue(diff.Actual.IsNotNullOrWhiteSpace());
            Assert.AreNotEqual(diff.Expected, diff.Actual);
        }

        [TestMethod]
        public void SpanTest()
        {
            var expected = $"test{Rng.GetInt64()}";
            var actual = new ReadOnlySpan<char>(expected.ToCharArray()).ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToCamelCaseTest()
        {
            var source = "to_camel case";
            var expected = "To Camel Case";
            var actual = source.ToCamelCase();
            Assert.AreEqual(expected, actual);
        }
    }
}