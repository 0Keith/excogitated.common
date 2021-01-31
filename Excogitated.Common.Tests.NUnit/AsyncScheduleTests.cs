using Excogitated.Common.Scheduling;
using Excogitated.Common.Scheduling.Execution;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
                .Execute()
                .MaxExecutions(expectedMaxExecutions)
                .Start(async expected =>
                {
                    var now = DateTimeOffset.Now;
                    var delta = Math.Abs(expected.Subtract(now).TotalMilliseconds);
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
                    .Execute()
                    .MaxRetries(10)
                    .Start(async expected =>
                    {
                        var now = DateTimeOffset.Now;
                        var delta = Math.Abs(expected.Subtract(now).TotalMilliseconds);
                        Assert.LessOrEqual(delta, maxDelta);
                        deltas.Add(delta);
                        throw new Exception(deltas.Count.ToString());
                    });
            }
            catch (Exception e)
            {
                LogStats(deltas);
                Console.WriteLine(e.Message);
                return;
            }
            Assert.Fail("Exception not caught.");
        }

        [Test]
        public async Task WithFileStore()
        {
            var deltas = new List<double>();
            var maxDelta = TimeSpan.FromSeconds(1).TotalMilliseconds;
            const int expectedMaxExecutions = 10;
            await Schedule.Build()
                .EveryMillisecond(100)
                .Execute()
                .MaxExecutions(expectedMaxExecutions)
                .WithFileStore("")
                .Start(async expected =>
                {
                    var now = DateTimeOffset.Now;
                    var delta = Math.Abs(expected.Subtract(now).TotalMilliseconds);
                    Assert.LessOrEqual(delta, maxDelta);
                    deltas.Add(delta);
                });
            LogStats(deltas);
            Assert.AreEqual(expectedMaxExecutions, deltas.Count);
        }

    }
}
