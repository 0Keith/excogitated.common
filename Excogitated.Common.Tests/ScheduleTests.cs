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
                .OnHourOfDayRange(5, 22)
                .GetEvents(now)
                .Take(24)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                var expected = now.AddHours(now.Hour == 22 ? 7 : now.Hour == 0 ? 5 : 1);
                Assert.AreEqual(expected.Hour, e.Hour);
                Assert.AreEqual(expected, e);
                now = expected;
            }
        }

        [TestMethod]
        public void HourOfDay_StartGreaterThanEnd()
        {
            var now = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnHourOfDayRange(22, 3)
                .GetEvents(now)
                .Take(24)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                var expected = now.AddHours(now.Hour == 3 ? 19 : 1);
                Assert.AreEqual(expected.Hour, e.Hour);
                Assert.AreEqual(expected, e);
                now = expected;
            }
        }

        [TestMethod]
        public void MinuteOfHour_StartLessThanEnd()
        {
            var now = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnMinuteOfHourRange(5, 22)
                .GetEvents(now)
                .Take(60)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                var expected = now.AddMinutes(now.Minute == 22 ? 43 : now.Minute == 0 ? 5 : 1);
                Assert.AreEqual(expected.Minute, e.Minute);
                Assert.AreEqual(expected, e);
                now = expected;
            }
        }

        [TestMethod]
        public void MinuteOfHour_StartGreaterThanEnd()
        {
            var now = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnMinuteOfHourRange(23, 3)
                .GetEvents(now)
                .Take(60)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                var expected = now.AddMinutes(now.Minute == 3 ? 20 : 1);
                Assert.AreEqual(expected.Minute, e.Minute);
                Assert.AreEqual(expected, e);
                now = expected;
            }
        }

        [TestMethod]
        public void SecondOfMinute_StartLessThanEnd()
        {
            var now = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnSecondOfMinuteRange(5, 22)
                .GetEvents(now)
                .Take(60)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                var expected = now.AddSeconds(now.Second == 22 ? 43 : now.Second == 0 ? 5 : 1);
                Assert.AreEqual(expected.Second, e.Second);
                Assert.AreEqual(expected, e);
                now = expected;
            }
        }

        [TestMethod]
        public void SecondOfMinute_StartGreaterThanEnd()
        {
            var now = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnSecondOfMinuteRange(23, 3)
                .GetEvents(now)
                .Take(60)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                var expected = now.AddSeconds(now.Second == 3 ? 20 : 1);
                Assert.AreEqual(expected.Second, e.Second);
                Assert.AreEqual(expected, e);
                now = expected;
            }
        }

        [TestMethod]
        public void MillisecondOfSecond_StartLessThanEnd()
        {
            var now = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnMillisecondOfSecondRange(5, 22)
                .GetEvents(now)
                .Take(1000)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                var expected = now.AddMilliseconds(now.Millisecond == 22 ? 983 : now.Millisecond == 0 ? 5 : 1);
                Assert.AreEqual(expected.Millisecond, e.Millisecond);
                Assert.AreEqual(expected, e);
                now = expected;
            }
        }

        [TestMethod]
        public void MillisecondOfSecond_StartGreaterThanEnd()
        {
            var now = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnMillisecondOfSecondRange(23, 3)
                .GetEvents(now)
                .Take(1000)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                var expected = now.AddMilliseconds(now.Millisecond == 3 ? 20 : 1);
                Assert.AreEqual(expected.Millisecond, e.Millisecond);
                Assert.AreEqual(expected, e);
                now = expected;
            }
        }
    }
}
