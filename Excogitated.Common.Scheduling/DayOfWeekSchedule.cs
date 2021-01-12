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
            _schedule = schedule ?? NullSchedule.Instance;
            _daysOfWeek = daysOfWeek?.ToHashSet() ?? new HashSet<DayOfWeek>();
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            if (_daysOfWeek.Count > 0)
                while (!_daysOfWeek.Contains(next.DayOfWeek))
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
        public static ISchedule OnDayOfWeek(this ISchedule schedule, params DayOfWeek[] daysOfWeek) => new DayOfWeekSchedule(schedule, daysOfWeek);
    }
}
