using System;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    public class TimeOfDayStartSchedule : ISchedule
    {
        private readonly TimeSpan _timeOfDay;
        private readonly ISchedule _schedule;

        public TimeOfDayStartSchedule(TimeSpan timeOfDay, ISchedule schedule = null)
        {
            _timeOfDay = timeOfDay;
            _schedule = schedule ?? NullSchedule.Instance;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            if (_timeOfDay.Count > 0)
                while (!_timeOfDay.Contains(next.TimeOfDay))
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
        public static ISchedule OnTimeOfDayStart(this ISchedule schedule, TimeSpan timeOfDay) => new TimeOfDayStartSchedule(timeOfDay, schedule);
    }
}