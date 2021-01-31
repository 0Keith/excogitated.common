using Excogitated.Common.Scheduling;
using NUnit.Framework;
using System;
using System.Linq;

namespace Excogitated.Common.Tests
{
    public class ScheduleTests : TestsBase
    {
        [Test]
        public void Interval()
        {
            var expected = DateTimeOffset.Now;
            var events = Schedule.Build()
                .EveryMillisecond(4)
                .EverySecond(5)
                .EveryMinute(6)
                .EveryHour(7)
                .EveryDay(8)
                .EveryMonth(9)
                .EveryYear(10)
                .GetEvents(expected)
                .Take(100)
                .ToList();
            foreach (var e in events)
            {
                expected = expected
                    .AddMilliseconds(4)
                    .AddSeconds(5)
                    .AddMinutes(6)
                    .AddHours(7)
                    .AddDays(8)
                    .AddMonths(9)
                    .AddYears(10);
                Console.WriteLine(e.ToString("O"));
                Assert.AreEqual(expected, e);
            }
        }

        [Test]
        public void HourOfDay_StartLessThanEnd()
        {
            var expected = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnHourOfDayRange(5, 22)
                .GetEvents(expected)
                .Take(24)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                expected = expected.AddHours(expected.Hour == 22 ? 7 : expected.Hour == 0 ? 5 : 1);
                Assert.AreEqual(expected.Hour, e.Hour);
                Assert.AreEqual(expected, e);
            }
        }

        [Test]
        public void HourOfDay_StartGreaterThanEnd()
        {
            var expected = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnHourOfDayRange(22, 3)
                .GetEvents(expected)
                .Take(24)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                expected = expected.AddHours(expected.Hour == 3 ? 19 : 1);
                Assert.AreEqual(expected.Hour, e.Hour);
                Assert.AreEqual(expected, e);
            }
        }

        [Test]
        public void MinuteOfHour_StartLessThanEnd()
        {
            var expected = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnMinuteOfHourRange(5, 22)
                .GetEvents(expected)
                .Take(60)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                expected = expected.AddMinutes(expected.Minute == 22 ? 43 : expected.Minute == 0 ? 5 : 1);
                Assert.AreEqual(expected.Minute, e.Minute);
                Assert.AreEqual(expected, e);
            }
        }

        [Test]
        public void MinuteOfHour_StartGreaterThanEnd()
        {
            var expected = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnMinuteOfHourRange(23, 3)
                .GetEvents(expected)
                .Take(60)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                expected = expected.AddMinutes(expected.Minute == 3 ? 20 : 1);
                Assert.AreEqual(expected.Minute, e.Minute);
                Assert.AreEqual(expected, e);
            }
        }

        [Test]
        public void SecondOfMinute_StartLessThanEnd()
        {
            var expected = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnSecondOfMinuteRange(5, 22)
                .GetEvents(expected)
                .Take(60)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                expected = expected.AddSeconds(expected.Second == 22 ? 43 : expected.Second == 0 ? 5 : 1);
                Assert.AreEqual(expected.Second, e.Second);
                Assert.AreEqual(expected, e);
            }
        }

        [Test]
        public void SecondOfMinute_StartGreaterThanEnd()
        {
            var expected = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnSecondOfMinuteRange(23, 3)
                .GetEvents(expected)
                .Take(60)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                expected = expected.AddSeconds(expected.Second == 3 ? 20 : 1);
                Assert.AreEqual(expected.Second, e.Second);
                Assert.AreEqual(expected, e);
            }
        }

        [Test]
        public void MillisecondOfSecond_StartLessThanEnd()
        {
            var expected = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnMillisecondOfSecondRange(5, 22)
                .GetEvents(expected)
                .Take(1000)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                expected = expected.AddMilliseconds(expected.Millisecond == 22 ? 983 : expected.Millisecond == 0 ? 5 : 1);
                Assert.AreEqual(expected.Millisecond, e.Millisecond);
                Assert.AreEqual(expected, e);
            }
        }

        [Test]
        public void MillisecondOfSecond_StartGreaterThanEnd()
        {
            var expected = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnMillisecondOfSecondRange(23, 3)
                .GetEvents(expected)
                .Take(1000)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                expected = expected.AddMilliseconds(expected.Millisecond == 3 ? 20 : 1);
                Assert.AreEqual(expected.Millisecond, e.Millisecond);
                Assert.AreEqual(expected, e);
            }
        }

        [Test]
        public void DayOfMonthSchedule()
        {
            var days = new[] { 3, 5, 7, 12, 29, 31 }.ToHashSet();
            var expected = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnDayOfMonth(days.ToArray())
                .GetEvents(expected)
                .Take(1000)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                expected = expected.AddDays(1);
                while (!days.Contains(expected.Day))
                    expected = expected.AddDays(1);
                Assert.AreEqual(expected, e);
            }
        }

        [Test]
        public void DayOfWeekSchedule()
        {
            var days = new[] { DayOfWeek.Friday, DayOfWeek.Monday, DayOfWeek.Sunday, DayOfWeek.Wednesday }.ToHashSet();
            var expected = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnDayOfWeek(days.ToArray())
                .GetEvents(expected)
                .Take(1000)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                expected = expected.AddDays(1);
                while (!days.Contains(expected.DayOfWeek))
                    expected = expected.AddDays(1);
                Assert.AreEqual(expected, e);
            }
        }

        [Test]
        public void DayOfYearSchedule()
        {
            var days = new[] { 3, 12, 123, 234, 345, 366 }.ToHashSet();
            var expected = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnDayOfYear(days.ToArray())
                .GetEvents(expected)
                .Take(1000)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                expected = expected.AddDays(1);
                while (!days.Contains(expected.DayOfYear))
                    expected = expected.AddDays(1);
                Assert.AreEqual(expected, e);
            }
        }

        [Test]
        public void NthDayOfWeekSchedule()
        {
            var expected = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnDayOfWeek(DayOfWeek.Friday, 4)
                .GetEvents(expected)
                .Take(1000)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
                expected = expected.AddDays(1);
                while (expected.DayOfWeek != DayOfWeek.Friday || !expected.IsFourthWeek())
                    expected = expected.AddDays(1);
                Assert.AreEqual(DayOfWeek.Friday, e.DayOfWeek);
                Assert.AreEqual(expected, e);
            }
        }

        [Test]
        public void HolidayOfYearSchedule()
        {
            var expected = new DateTimeOffset(1985, 12, 17, 0, 0, 0, TimeSpan.FromHours(-6));
            var events = Schedule.Build()
                .OnHolidayOfYear()
                .GetEvents(expected)
                .Take(1000)
                .ToList();
            foreach (var e in events)
            {
                Console.WriteLine(e.ToString("O"));
            }
        }
    }
}
