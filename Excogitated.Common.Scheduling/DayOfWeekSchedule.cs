using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    public class DayOfWeekSchedule : ISchedule
    {
        private readonly HashSet<DayOfWeek> _daysOfWeek;
        private readonly ISchedule _schedule;

        public DayOfWeekSchedule(DayOfWeek[] days, ISchedule schedule = null)
        {
            _daysOfWeek = days?.ToHashSet() ?? new HashSet<DayOfWeek>();
            _schedule = schedule ?? NullSchedule.Instance;
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
        public static ISchedule OnDayOfWeek(this ISchedule schedule, params DayOfWeek[] days) => new DayOfWeekSchedule(days, schedule);
    }
}
