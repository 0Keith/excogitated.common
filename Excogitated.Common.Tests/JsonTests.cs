using Excogitated.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Test
{

    [TestClass]
    public class JsonTests
    {
        public static TestItem0 Item { get; } = new TestItem0
        {
            mdy = Date.Today,
            Now = DateTime.Now,
            NowOffset = DateTimeOffset.Now,
            Version = 0,
            AverageVolume = Rng.Pseudo.GetDouble(),
            Codes = Enumerable.Range(0, 10).Select(i => Guid.NewGuid().ToString()).ToList(),
            ListDate = Date.Today,
            MarginRatio = Rng.Pseudo.GetInt32(),
            Name = "Test Instrument Document",
            //Prices = Enumerable.Range(0, 2).Select(i => new TestItem1
            Prices = Enumerable.Range(0, 5 * 365).Select(i => new TestItem1
            {
                Close = Rng.Pseudo.GetDecimal(),
                Date = Date.Today.AddDays(-i),
                High = Rng.Pseudo.GetDecimal(),
                Low = Rng.Pseudo.GetDecimal(),
                Open = Rng.Pseudo.GetDecimal(),
                Type = Rng.Pseudo.SelectOne<TestEnum0>(),
                Volume = Rng.Pseudo.GetInt64(),
            }).ToList(),
            Symbol = "TEST",
            Type = Rng.Pseudo.SelectOne<TestEnum1>(),
            //Dividends = Enumerable.Range(0, 2).Select(i => new TestItem2
            Dividends = Enumerable.Range(0, 5 * 12).Select(i => new TestItem2
            {
                Amount = Rng.Pseudo.GetDecimal(),
                DeclaredDate = Date.Today.AddMonths(-i),
                ExDate = Date.Today.AddDays(7).AddMonths(-i),
                PayDate = Date.Today.AddDays(14).AddMonths(-i),
                Type = Rng.Pseudo.SelectOne<TestEnum2>()
            }).ToList(),
            DividendsFullUpdate = Date.Today.AddDays(-5),
        };

        [TestMethod]
        public void CompareJson()
        {
            var expected = Jsonizer.Serialize(Item);
            Console.WriteLine($"Expected: {expected}\n");
            var clone = Jsonizer.Deserialize<TestItem0>(expected);
            var actual = Jsonizer.Serialize(clone);
            Console.WriteLine($"  Actual: {actual}\n");
            Assert.AreEqual(expected.GetHashCode(), actual.GetHashCode(), expected.Diff(actual).ToString());
        }

        [TestMethod]
        public void CompareJsonFormatted()
        {
            var expected = Jsonizer.Serialize(Item, true);
            Console.WriteLine($"Expected: {expected}\n");
            var clone = Jsonizer.Deserialize<TestItem0>(expected);
            var actual = Jsonizer.Serialize(clone, true);
            Console.WriteLine($"  Actual: {actual}\n");
            Assert.AreEqual(expected.GetHashCode(), actual.GetHashCode(), expected.Diff(actual).ToString());
        }

        [TestMethod]
        public void CompareJsonSerializePerformance()
        {
            var r1 = Benchmark.Run(() => Newtonsoft.Json.JsonConvert.SerializeObject(Item));
            var r3 = Benchmark.Run(() => System.Text.Json.JsonSerializer.Serialize(Item));
            var r4 = Benchmark.Run(() => Jsonizer.Serialize(Item));
            var stats = new StatsBuilder()
                .Add(typeof(Newtonsoft.Json.JsonConvert).FullName, r1)
                .Add(typeof(System.Text.Json.JsonSerializer).FullName, r3)
                .Add(typeof(Jsonizer).FullName, r4)
                .ToString();
            Console.WriteLine(stats);
            Assert.IsTrue(r4.PerSecond > r1.PerSecond);
        }

        [TestMethod]
        public void CompareJsonDeserializePerformance()
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(Item);
            var r1 = Benchmark.Run(() => Newtonsoft.Json.JsonConvert.DeserializeObject<TestItem0>(json));

            json = System.Text.Json.JsonSerializer.Serialize(Item);
            var r3 = Benchmark.Run(() => System.Text.Json.JsonSerializer.Deserialize<TestItem0>(json));

            json = Jsonizer.Serialize(Item);
            var r4 = Benchmark.Run(() => Jsonizer.Deserialize<TestItem0>(json));
            var stats = new StatsBuilder()
                .Add(typeof(Newtonsoft.Json.JsonConvert).FullName, r1)
                .Add(typeof(System.Text.Json.JsonSerializer).FullName, r3)
                .Add(typeof(Jsonizer).FullName, r4)
                .ToString();
            Console.WriteLine(stats);
            Assert.IsTrue(r4.PerSecond > r1.PerSecond);
        }

        [TestMethod]
        public void EscapeFileNameTest()
        {
            var fileName = new string(Enumerable.Range(char.MinValue, char.MaxValue + char.MaxValue).Randomize().Select(i => (char)i).ToArray());
            var escaped = fileName.EscapeFileName();
            var unescaped = escaped.UnescapeFileName();
            Assert.AreEqual(fileName.Length, unescaped.Length);
            Assert.IsTrue(fileName == unescaped);
        }

        [TestMethod]
        public void CompareDecimalDeserializePerformance()
        {
            var json = Rng.Pseudo.GetDecimal().ToString();
            var r1 = Benchmark.Run(() => decimal.Parse(json));
            json = $"${json}";
            var r2 = Benchmark.Run(() => json.ToDecimal());
            var stats = new StatsBuilder()
                .Add(nameof(decimal.Parse), r1)
                .Add(nameof(Extensions_String.ToDecimal), r2)
                .ToString();
            Console.WriteLine(stats);
        }
    }

    public enum TestEnum2 { test1, test2, test3, test4, test5, Test5 = test5 }
    public enum TestEnum1 { test1, test2, test3, test4, test5, Test5 = test5 }
    public enum TestEnum0 { test1, test2, test3, test4, test5, Test5 = test5 }

    public class TestItem2
    {
        public decimal Amount { get; set; }
        public Date DeclaredDate { get; set; }
        public Date ExDate { get; set; }
        public Date PayDate { get; set; }
        public TestEnum2 Type { get; set; }
    }

    public class TestItem1
    {
        public decimal Close { get; set; }
        public Date Date { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Open { get; set; }
        public TestEnum0 Type { get; set; }
        public long Volume { get; set; }
    }

    public class TestItem0
    {
        public MonthDayYear mdy { get; set; }
        public MonthDayYear MonthDayYear => mdy;

        public DateTime Now { get; set; }
        public DateTimeOffset NowOffset { get; set; }
        public int Version { get; set; }
        public double AverageVolume { get; set; }
        public List<string> Codes { get; set; }
        public Date ListDate { get; set; }
        public int MarginRatio { get; set; }
        public string Name { get; set; }
        public List<TestItem1> Prices { get; set; }
        public string Symbol { get; set; }
        public TestEnum1 Type { get; set; }
        public List<TestItem2> Dividends { get; set; }
        public Date DividendsFullUpdate { get; set; }
    }
}