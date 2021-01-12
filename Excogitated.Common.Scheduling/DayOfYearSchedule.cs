using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    internal class DayOfYearSchedule : ISchedule
    {
        private readonly ISchedule _schedule;
        private readonly HashSet<int> _daysOfYear;

        public DayOfYearSchedule(ISchedule schedule, int[] daysOfYear)
        {
            if (daysOfYear is object)
                foreach (var dayOfYear in daysOfYear)
                    if (dayOfYear < 1 || dayOfYear > 366)
                        throw new ArgumentException("dayOfYear < 1 || dayOfYear > 366", nameof(dayOfYear));
            _schedule = schedule ?? NullSchedule.Instance;
            _daysOfYear = daysOfYear?.ToHashSet() ?? new HashSet<int>();
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            if (_daysOfYear.Count > 0)
                while (!_daysOfYear.Contains(next.DayOfYear))
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
        public static ISchedule OnDayOfYear(this ISchedule schedule, params int[] daysOfYear) => new DayOfYearSchedule(schedule, daysOfYear);
    }
}