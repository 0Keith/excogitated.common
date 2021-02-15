using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    internal class DayOfMonthSchedule : ISchedule
    {
        private readonly ISchedule _schedule;
        private readonly HashSet<int> _daysOfMonth;

        public DayOfMonthSchedule(ISchedule schedule, int[] daysOfMonth)
        {
            if (daysOfMonth is object)
                foreach (var dayOfMonth in daysOfMonth)
                    if (dayOfMonth < 1 || dayOfMonth > 31)
                        throw new ArgumentException("dayOfMonth < 1 || dayOfMonth > 31", nameof(dayOfMonth));
            _schedule = schedule.OrEvery(TimeUnit.Day);
            _daysOfMonth = daysOfMonth?.ToHashSet() ?? new HashSet<int>();
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            if (_daysOfMonth.Count > 0)
                while (!_daysOfMonth.Contains(next.Day))
                    next = _schedule.GetNextEvent(next);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnDayOfMonth(this ISchedule schedule, params int[] daysOfMonth) => new DayOfMonthSchedule(schedule, daysOfMonth);
    }
}