using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    public class TimeOfDayEndSchedule : ISchedule
    {
        private readonly HashSet<TimeSpan> _timesOfDay;
        private readonly ISchedule _schedule;

        public TimeOfDayEndSchedule(TimeSpan[] timesOfDay, ISchedule schedule = null)
        {
            _timesOfDay = timesOfDay?.OrderBy(t => t. .ToHashSet() ?? new HashSet<TimeSpan>();
            _schedule = schedule ?? NullSchedule.Instance;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            if (_timesOfDay.Count > 0)
                while (!_timesOfDay.Contains(next.TimeOfDay))
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
        public static ISchedule OnTimeOfDayEnd(this ISchedule schedule, params TimeSpan[] timesOfDay) => new TimeOfDayEndSchedule(timesOfDay, schedule);
    }
}