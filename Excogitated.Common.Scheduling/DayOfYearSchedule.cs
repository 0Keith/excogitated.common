using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    public class DayOfYearSchedule : ISchedule
    {
        private readonly HashSet<int> _daysOfYear;
        private readonly ISchedule _schedule;

        public DayOfYearSchedule(int[] daysOfYear, ISchedule schedule = null)
        {
            _daysOfYear = daysOfYear?.ToHashSet() ?? new HashSet<int>();
            _schedule = schedule ?? NullSchedule.Instance;
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
        public static ISchedule OnDayOfYear(this ISchedule schedule, params int[] daysOfYear) => new DayOfYearSchedule(daysOfYear, schedule);
    }
}