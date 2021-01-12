using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    internal class SecondOfMinuteSchedule : ISchedule
    {
        private readonly ISchedule _schedule;
        private readonly HashSet<int> _secondsOfMinute;

        public SecondOfMinuteSchedule(ISchedule schedule, int[] secondsOfMinute)
        {
            if (secondsOfMinute is object)
                foreach (var secondOfMinute in secondsOfMinute)
                    if (secondOfMinute < 0 || secondOfMinute > 59)
                        throw new ArgumentException($"secondOfMinute < 0 || secondOfMinute > 59", nameof(secondOfMinute));
            _schedule = schedule ?? NullSchedule.Instance;
            _secondsOfMinute = secondsOfMinute?.ToHashSet() ?? new HashSet<int>();
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            if (_secondsOfMinute.Count > 0)
                while (!_secondsOfMinute.Contains(next.Second))
                    next = GetNextEventPrivate(next);
            return next;
        }

        private DateTimeOffset GetNextEventPrivate(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            if (previousEvent == next)
                return next.AddSeconds(1);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnSecondOfMinute(this ISchedule schedule, params int[] secondsOfMinute) => new SecondOfMinuteSchedule(schedule, secondsOfMinute);

        public static ISchedule OnSecondOfMinuteRange(this ISchedule schedule, int secondOfMinuteStart, int secondOfMinuteEnd)
        {
            var secondsOfMinute = secondOfMinuteStart.GetRange(secondOfMinuteEnd, 60).ToArray();
            return new SecondOfMinuteSchedule(schedule, secondsOfMinute);
        }
    }
}