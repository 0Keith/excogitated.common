using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Scheduling
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

        public DateTimeOffset GetNextEvent(DateTimeOffset start)
        {
            var next = _schedule.GetNextEvent(start);
            if (_daysOfWeek.Count > 0)
                while (!_daysOfWeek.Contains(next.DayOfWeek))
                    next = _schedule.GetNextEvent(next);
            return next;
        }

        public DateTimeOffset GetPreviousEvent(DateTimeOffset start)
        {
            var previous = _schedule.GetPreviousEvent(start);
            if (_daysOfWeek.Count > 0)
                while (!_daysOfWeek.Contains(previous.DayOfWeek))
                    previous = _schedule.GetPreviousEvent(previous);
            return previous;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnDayOfWeek(this ISchedule schedule, params DayOfWeek[] daysOfWeek) => new DayOfWeekSchedule(schedule, daysOfWeek);
    }
}
