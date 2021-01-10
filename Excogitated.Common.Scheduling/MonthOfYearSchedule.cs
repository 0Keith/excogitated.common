using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    public class MonthOfYearSchedule : ISchedule
    {
        private readonly HashSet<int> _monthsOfYear;
        private readonly ISchedule _schedule;

        public MonthOfYearSchedule(int[] months, ISchedule schedule = null)
        {
            _monthsOfYear = months?.ToHashSet() ?? new HashSet<int>();
            _schedule = schedule ?? NullSchedule.Instance;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            if (_monthsOfYear.Count > 0)
                while (!_monthsOfYear.Contains(next.Month))
                    next = GetNextEventPrivate(next);
            return next;
        }

        private DateTimeOffset GetNextEventPrivate(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            if (previousEvent == next)
                return next.AddMonths(1);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnMonthOfYear(this ISchedule schedule, params int[] months) => new MonthOfYearSchedule(months, schedule);

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
