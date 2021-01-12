using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    public class MinuteOfHourSchedule : ISchedule
    {
        private readonly HashSet<int> _minutesOfHour;
        private readonly ISchedule _schedule;

        public MinuteOfHourSchedule(int[] minutesOfHour, ISchedule schedule = null)
        {
            if (minutesOfHour is object)
                foreach (var minuteOfHour in minutesOfHour)
                    if (minuteOfHour < 0 || minuteOfHour > 59)
                        throw new ArgumentException("minuteOfHour < 0 || minuteOfHour > 59", nameof(minuteOfHour));
            _minutesOfHour = minutesOfHour?.ToHashSet() ?? new HashSet<int>();
            _schedule = schedule ?? NullSchedule.Instance;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            if (_minutesOfHour.Count > 0)
                while (!_minutesOfHour.Contains(next.Minute))
                    next = GetNextEventPrivate(next);
            return next;
        }

        private DateTimeOffset GetNextEventPrivate(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            if (previousEvent == next)
                return next.AddMinutes(1);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnMinuteOfHour(this ISchedule schedule, params int[] minutesOfHour) => new MinuteOfHourSchedule(minutesOfHour, schedule);

        public static ISchedule OnMinuteOfHourRange(this ISchedule schedule, int minuteOfHourStart, int minuteOfHourEnd)
        {
            var minutesOfHour = minuteOfHourStart.GetRange(minuteOfHourEnd, 60).ToArray();
            return new MinuteOfHourSchedule(minutesOfHour, schedule);
        }
    }
}