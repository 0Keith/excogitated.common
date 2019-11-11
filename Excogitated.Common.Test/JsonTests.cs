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
        private readonly TestItem0 _item = new TestItem0
        {
            Version = 0,
            AverageVolume = Rng.Pseudo.GetDouble(),
            Codes = Enumerable.Range(0, 10).Select(i => Guid.NewGuid().ToString()).ToList(),
            ListDate = Date.Today,
            MarginRatio = Rng.Pseudo.GetInt32(),
            Name = "Test Instrument Document",
            //Prices = Enumerable.Range(0, 2).Select(i => new PriceDocument
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
            //Dividends = Enumerable.Range(0, 2).Select(i => new DividendDocument
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
            var expected = Jsonizer.Serialize(_item);
            Console.WriteLine($"Expected: {expected}\n");
            var clone = Jsonizer.Deserialize<TestItem0>(expected);
            var actual = Jsonizer.Serialize(clone);
            Console.WriteLine($"  Actual: {actual}\n");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CompareJsonFormatted()
        {
            var expected = Jsonizer.Serialize(_item, true);
            Console.WriteLine($"Expected: {expected}\n");
            var clone = Jsonizer.Deserialize<TestItem0>(expected);
            var actual = Jsonizer.Serialize(clone, true);
            Console.WriteLine($"  Actual: {actual}\n");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CompareJsonSerializePerformance()
        {
            var r1 = Benchmark.Run(() => Newtonsoft.Json.JsonConvert.SerializeObject(_item));
            var r2 = Benchmark.Run(() => Utf8Json.JsonSerializer.ToJsonString(_item));
            var r3 = Benchmark.Run(() => System.Text.Json.JsonSerializer.Serialize(_item));
            var r4 = Benchmark.Run(() => Jsonizer.Serialize(_item));
            var stats = new StatsBuilder()
                .Add(typeof(Newtonsoft.Json.JsonConvert).FullName, r1)
                .Add(typeof(Utf8Json.JsonSerializer).FullName, r2)
                .Add(typeof(System.Text.Json.JsonSerializer).FullName, r3)
                .Add(typeof(Jsonizer).FullName, r4)
                .ToString();
            Console.WriteLine(stats);
        }

        [TestMethod]
        public void CompareJsonDeserializePerformance()
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_item);
            var r1 = Benchmark.Run(() => Newtonsoft.Json.JsonConvert.DeserializeObject<TestItem0>(json));

            json = Utf8Json.JsonSerializer.ToJsonString(_item);
            var r2 = Benchmark.Run(() => Utf8Json.JsonSerializer.Deserialize<TestItem0>(json));

            json = System.Text.Json.JsonSerializer.Serialize(_item);
            var r3 = Benchmark.Run(() => System.Text.Json.JsonSerializer.Deserialize<TestItem0>(json));

            json = Jsonizer.Serialize(_item);
            var r4 = Benchmark.Run(() => Jsonizer.Deserialize<TestItem0>(json));
            var stats = new StatsBuilder()
                .Add(typeof(Newtonsoft.Json.JsonConvert).FullName, r1)
                .Add(typeof(Utf8Json.JsonSerializer).FullName, r2)
                .Add(typeof(System.Text.Json.JsonSerializer).FullName, r3)
                .Add(typeof(Jsonizer).FullName, r4)
                .ToString();
            Console.WriteLine(stats);
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

    internal enum TestEnum2 { test1, test2, test3, test4, test5 }
    internal enum TestEnum1 { test1, test2, test3, test4, test5 }
    internal enum TestEnum0 { test1, test2, test3, test4, test5 }

    internal class TestItem2
    {
        public decimal Amount { get; set; }
        public Date DeclaredDate { get; set; }
        public Date ExDate { get; set; }
        public Date PayDate { get; set; }
        public TestEnum2 Type { get; set; }
    }

    internal class TestItem1
    {
        public decimal Close { get; set; }
        public Date Date { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Open { get; set; }
        public TestEnum0 Type { get; set; }
        public long Volume { get; set; }
    }

    internal class TestItem0
    {
        public int Version { get; internal set; }
        public double AverageVolume { get; internal set; }
        public List<string> Codes { get; internal set; }
        public Date ListDate { get; internal set; }
        public int MarginRatio { get; internal set; }
        public string Name { get; internal set; }
        public List<TestItem1> Prices { get; internal set; }
        public string Symbol { get; internal set; }
        public TestEnum1 Type { get; internal set; }
        public List<TestItem2> Dividends { get; internal set; }
        public Date DividendsFullUpdate { get; internal set; }
    }
}