using Excogitated.Common.Scheduling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Excogitated.Common.Tests
{
    [TestClass]
    public class ScheduleTests : TestsBase
    {
        [TestMethod]
        public void Interval()
        {
            var now = DateTimeOffset.Now;
            var events = Schedule.Build()
                .EveryMillisecond(4)
                .EverySecond(5)
                .EveryMinute(6)
                .EveryHour(7)
                .EveryDay(8)
                .EveryMonth(9)
                .EveryYear(10)
                .GetEvents(now)
                .Take(100)
                .ToList();
            foreach (var e in events)
            {
                var expected = now
                    .AddMilliseconds(4)
                    .AddSeconds(5)
                    .AddMinutes(6)
                    .AddHours(7)
                    .AddDays(8)
                    .AddMonths(9)
                    .AddYears(10);
                Console.WriteLine(e.ToString("O"));
                Assert.AreEqual(expected, e);
                now = expected;
            }
        }

        [TestMethod]
        public void On()
        {
            var now = DateTimeOffset.Now;
            var events = Schedule.Build()
                .
                .GetEvents(now)
                .Take(100)
                .ToList();
            foreach (var e in events)
            {
                var expected = now
                    .AddMilliseconds(4)
                    .AddSeconds(5)
                    .AddMinutes(6)
                    .AddHours(7)
                    .AddDays(8)
                    .AddMonths(9)
                    .AddYears(10);
                Console.WriteLine(e.ToString("O"));
                Assert.AreEqual(expected, e);
                now = expected;
            }
        }
    }
}
