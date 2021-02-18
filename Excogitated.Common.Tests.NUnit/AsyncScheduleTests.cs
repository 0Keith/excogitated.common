using Excogitated.Common.Extensions;
using Excogitated.Common.Scheduling;
using Excogitated.Common.Scheduling.Execution;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common.Tests
{
    public class AsyncScheduleTests : TestsBase
    {
        private static void LogStats(List<double> deltas)
        {
            Console.WriteLine(new
            {
                Deltas = new
                {
                    Max = deltas.Max(),
                    Min = deltas.Min(),
                    Average = deltas.Average(),
                    deltas.Count
                }
            });
        }

        [Test]
        public async Task MaxExecutions()
        {
            var deltas = new List<double>();
            var maxDelta = TimeSpan.FromSeconds(1).TotalMilliseconds;
            const int expectedMaxExecutions = 10;
            await Schedule.Build()
                .EveryMillisecond(100)
                .BuildJob()
                .MaxExecutions(expectedMaxExecutions)
                .Start(async context =>
                {
                    var now = DateTimeOffset.Now;
                    var delta = Math.Abs(context.Expected.Subtract(now).TotalMilliseconds);
                    Assert.LessOrEqual(delta, maxDelta);
                    deltas.Add(delta);
                });
            LogStats(deltas);
            Assert.AreEqual(expectedMaxExecutions, deltas.Count);
        }

        [Test]
        public async Task MaxRetries()
        {
            var deltas = new List<double>();
            var maxDelta = TimeSpan.FromSeconds(1).TotalMilliseconds;
            try
            {
                await Schedule.Build()
                    .EveryMillisecond(100)
                    .BuildJob()
                    .MaxRetries(10)
                    .Start(async context =>
                    {
                        var now = DateTimeOffset.Now;
                        var delta = Math.Abs(context.Expected.Subtract(now).TotalMilliseconds);
                        Assert.LessOrEqual(delta, maxDelta);
                        deltas.Add(delta);
                        throw new Exception(deltas.Count.ToString());
                    });
            }
            catch (Exception e)
            {
                LogStats(deltas);
                Console.WriteLine(e.Message);
                Assert.AreEqual(10, deltas.Count);
                return;
            }
            Assert.Fail("Exception not caught.");
        }

        [Test]
        public async Task WithFileStore()
        {
            var file = new FileInfo("./schedule.test.json");
            if (file.Exists)
                await file.DeleteAsync();
            var expected = DateTimeOffset.Now;
            await Schedule.Build()
                .EveryMillisecond(100)
                .BuildJob()
                .MaxExecutions(1)
                .WithFileStore(file.FullName)
                .Start(async context =>
                {
                    file.Refresh();
                    Assert.IsFalse(file.Exists);
                    expected = context.Expected;
                });
            var actual = DateTimeOffset.Parse(await File.ReadAllTextAsync(file.FullName));
            Assert.AreEqual(expected, actual);

            await Task.Delay(1000);
            await Schedule.Build()
                .EveryMillisecond(100)
                .BuildJob()
                .MaxExecutions(1)
                .WithFileStore(file.FullName)
                .Start(async context =>
                {
                    Assert.AreEqual(10, context.MissedEvents.Count);
                    var delta = context.Expected.Subtract(actual);
                    Assert.AreEqual(context.Expected, actual);
                });
        }
    }
}
