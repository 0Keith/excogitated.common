using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    public class DayOfMonthSchedule : ISchedule
    {
        private readonly HashSet<int> _daysOfMonth;
        private readonly ISchedule _schedule;

        public DayOfMonthSchedule(int[] daysOfMonth, ISchedule schedule = null)
        {
            _daysOfMonth = daysOfMonth?.ToHashSet() ?? new HashSet<int>();
            _schedule = schedule ?? NullSchedule.Instance;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            if (_daysOfMonth.Count > 0)
                while (!_daysOfMonth.Contains(next.Day))
                    next = GetNextEventPrivate(next);
            return next;
        }

        private DateTimeOffset GetNextEventPrivate(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            if (previousEvent == next)
                return next.AddDays(1);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnDayOfMonth(this ISchedule schedule, params int[] daysOfMonth) => new MonthOfYearSchedule(daysOfMonth, schedule);
    }
}