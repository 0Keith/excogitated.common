using Excogitated.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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
                Type = Rng.Pseudo.SelectOne<TestEnum2>(),
                Price = Rng.Pseudo.GetDecimal(),
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

        [TestMethod]
        public void GenerateClassFromJson()
        {
            var json = "{\"next\":\"https://api.robinhood.com/options/orders/?cursor=cD0yMDE5LTA0LTIyKzE3JTNBMzElM0E1Ni44MjQzNzYlMkIwMCUzQTAw\",\"previous\":null,\"results\":[{\"cancel_url\":null,\"canceled_quantity\":\"0.00000\",\"created_at\":\"2020-02-28T17:55:05.955191Z\",\"direction\":\"credit\",\"id\":\"e2d8dc8d-6637-469c-9b27-61499069d300\",\"legs\":[{\"executions\":[{\"id\":\"5bba6932-93c7-4e77-91ee-c3da36106f73\",\"price\":\"$6.85000000\",\"quantity\":\"2.00000\",\"settlement_date\":\"2020-03-02\",\"timestamp\":\"2020-02-28T21:00:00.059000Z\"},{\"id\":\"c730e53c-eaf8-4b30-b258-344ff7d3bf21\",\"price\":\"$6.85000000\",\"quantity\":\"2.00000\",\"settlement_date\":\"2020-03-02\",\"timestamp\":\"2020-02-28T21:02:17.765000Z\"},{\"id\":\"2da9a6b4-61a8-4726-a42d-e43c68333b0b\",\"price\":\"6.85000000\",\"quantity\":\"6.00000\",\"settlement_date\":\"2020-03-02\",\"timestamp\":\"2020-02-28T21:02:17.773000Z\"},{\"id\":\"e202d522-0eb9-4932-8323-c030368a20eb\",\"price\":\"6.85000000\",\"quantity\":\"2.00000\",\"settlement_date\":\"2020-03-02\",\"timestamp\":\"2020-02-28T21:02:17.609000Z\"}],\"id\":\"c91e3aed-ace2-488b-9cb6-19acc8e64b44\",\"option\":\"https://api.robinhood.com/options/instruments/2de556d4-fde0-43a3-83bf-9ff8aaa41d46/\",\"position_effect\":\"close\",\"ratio_quantity\":1,\"side\":\"sell\"}],\"pending_quantity\":\"0.00000\",\"premium\":\"685.00000000\",\"processed_premium\":\"8220.00000000000000000\",\"price\":\"6.85000000\",\"processed_quantity\":\"12.00000\",\"quantity\":\"12.00000\",\"ref_id\":\"560047e3-a3ee-47a6-858e-17c5cab67bc7\",\"state\":\"filled\",\"time_in_force\":\"gfd\",\"trigger\":\"immediate\",\"type\":\"limit\",\"updated_at\":\"2020-02-28T21:02:18.447276Z\",\"chain_id\":\"c277b118-58d9-4060-8dc5-a3b5898955cb\",\"chain_symbol\":\"SPY\",\"response_category\":\"success\",\"opening_strategy\":null,\"closing_strategy\":\"long_call\",\"stop_price\":null}]}";
            var result = new JsonClassGenerator
            {
                RootName = "MarketdataOptions"
            }.FromString(json);
            Console.WriteLine(result);
            Console.WriteLine(Jsonizer.Format(json));
        }

        [TestMethod]
        public async Task GenerateClassFromJsonUrl()
        {
            using var client = new HttpClient();
            var json = await client.GetStringAsync("https://api.nasdaq.com/api/quote/A/info?assetclass=stocks");
            var result = new JsonClassGenerator()
            {
                RootName = "OptionInstruments"
            }.FromString(json);
            Console.WriteLine(result);
            Console.WriteLine(Jsonizer.Format(json));
        }

        [TestMethod]
        public async Task JsonObjectOutput()
        {
            var item = new TestItem2 { Amount = Rng.Pseudo.GetDecimal() };
            var expected = Jsonizer.Serialize(item, true);
            var actual = item.ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void StringToNumberTest()
        {
            var item = Jsonizer.Deserialize<TestItem2>("{\"Amount\":\"1234.5678\"}");
            Assert.AreEqual(1234.5678m, item.Amount);
        }

        [TestMethod]
        public void NullToNumberTest()
        {
            var item = Jsonizer.Deserialize<TestItem2>("{\"Amount\":null}");
            Assert.AreEqual(0m, item.Amount);
        }
    }

    public enum TestEnum2 { test1, test2, test3, test4, test5, Test5 = test5 }
    public enum TestEnum1 { test1, test2, test3, test4, test5, Test5 = test5 }
    public enum TestEnum0 { test1, test2, test3, test4, test5, Test5 = test5 }

    public class TestItem2 : JsonObject
    {
        public decimal Amount { get; set; }
        public Date DeclaredDate { get; set; }
        public Date ExDate { get; set; }
        public Date PayDate { get; set; }
        public TestEnum2 Type { get; set; }
        public Currency Price { get; set; }
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
        public double AverageVolume { get; set; }
        public List<string> Codes { get; set; }
        public Date ListDate { get; set; }
        public int MarginRatio { get; set; }
        public string Name { get; set; }
        public List<TestItem2> Dividends { get; set; }
        public List<TestItem1> Prices { get; set; }
        public string Symbol { get; set; }
        public TestEnum1 Type { get; set; }
        public Date DividendsFullUpdate { get; set; }
    }
}