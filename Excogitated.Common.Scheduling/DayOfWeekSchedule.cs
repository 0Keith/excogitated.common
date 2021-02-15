using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    internal class DayOfWeekSchedule : ISchedule
    {
        private readonly ISchedule _schedule;
        private readonly HashSet<DayOfWeek> _daysOfWeek;

        public DayOfWeekSchedule(ISchedule schedule, DayOfWeek[] daysOfWeek)
        {
            _schedule = schedule.OrEvery(TimeUnit.Day);
            _daysOfWeek = daysOfWeek?.ToHashSet() ?? new HashSet<DayOfWeek>();
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            if (_daysOfWeek.Count > 0)
                while (!_daysOfWeek.Contains(next.DayOfWeek))
                    next = _schedule.GetNextEvent(next);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnDayOfWeek(this ISchedule schedule, params DayOfWeek[] daysOfWeek) => new DayOfWeekSchedule(schedule, daysOfWeek);
    }
}
