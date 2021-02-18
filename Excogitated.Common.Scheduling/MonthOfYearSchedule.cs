using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    internal class MonthOfYearSchedule : ISchedule
    {
        private readonly ISchedule _schedule;
        private readonly HashSet<int> _monthsOfYear;

        public MonthOfYearSchedule(ISchedule schedule, int[] months)
        {
            if (months is object)
                foreach (var month in months)
                    if (month < 1 || month > 12)
                        throw new ArgumentException("month < 1 || month > 12", nameof(month));
            _schedule = schedule.OrEvery(TimeUnit.Month);
            _monthsOfYear = months?.ToHashSet() ?? new HashSet<int>();
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset start)
        {
            var next = _schedule.GetNextEvent(start);
            if (_monthsOfYear.Count > 0)
                while (!_monthsOfYear.Contains(next.Month))
                    next = _schedule.GetNextEvent(next);
            return next;
        }

        public DateTimeOffset GetPreviousEvent(DateTimeOffset start)
        {
            var previous = _schedule.GetPreviousEvent(start);
            if (_monthsOfYear.Count > 0)
                while (!_monthsOfYear.Contains(previous.Month))
                    previous = _schedule.GetPreviousEvent(previous);
            return previous;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnMonthOfYear(this ISchedule schedule, params int[] months) => new MonthOfYearSchedule(schedule, months);

        public static ISchedule OnMonthOfYear(this ISchedule schedule, params MonthOfYear[] months) => schedule.OnMonthOfYear(months?.Cast<int>().ToArray());
    }

    public enum MonthOfYear
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }
}
