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
        public void HourOfDay_StartLessThanEnd()
        {
            var now = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnHourOfDay(5, 22)
                .GetEvents(now)
                .Take(24)
                .ToList();
            foreach (var e in events)
            {
                var expected = now.AddHours(now.Hour == 22 ? 7 : now.Hour == 0 ? 5 : 1);
                Console.WriteLine(e.ToString("O"));
                Assert.AreEqual(expected, e);
                now = expected;
            }
        }

        [TestMethod]
        public void HourOfDay_StartGreaterThanEnd()
        {
            var now = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnHourOfDay(22, 3)
                .GetEvents(now)
                .Take(24)
                .ToList();
            foreach (var e in events)
            {
                var expected = now.AddHours(now.Hour == 3 ? 19 : 1);
                Console.WriteLine(e.ToString("O"));
                Assert.AreEqual(expected, e);
                now = expected;
            }
        }
    }
}
