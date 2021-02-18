using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    internal class MillisecondOfSecondSchedule : ISchedule
    {
        private readonly ISchedule _schedule;
        private readonly HashSet<int> _millisecondsOfSecond;

        public MillisecondOfSecondSchedule(ISchedule schedule, int[] millisecondsOfSecond)
        {
            if (millisecondsOfSecond is object)
                foreach (var millisecondOfSecond in millisecondsOfSecond)
                    if (millisecondOfSecond < 0 || millisecondOfSecond > 999)
                        throw new ArgumentException("millisecondOfSecond < 0 || millisecondOfSecond > 999", nameof(millisecondOfSecond));
            _schedule = schedule.OrEvery(TimeUnit.Millisecond);
            _millisecondsOfSecond = millisecondsOfSecond?.ToHashSet() ?? new HashSet<int>();
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset start)
        {
            var next = _schedule.GetNextEvent(start);
            if (_millisecondsOfSecond.Count > 0)
                while (!_millisecondsOfSecond.Contains(next.Millisecond))
                    next = _schedule.GetNextEvent(next);
            return next;
        }

        public DateTimeOffset GetPreviousEvent(DateTimeOffset start)
        {
            var next = _schedule.GetPreviousEvent(start);
            if (_millisecondsOfSecond.Count > 0)
                while (!_millisecondsOfSecond.Contains(next.Millisecond))
                    next = _schedule.GetPreviousEvent(next);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnMillisecondOfSecond(this ISchedule schedule, params int[] millisecondsOfSecond)
        {
            return new MillisecondOfSecondSchedule(schedule, millisecondsOfSecond);
        }

        public static ISchedule OnMillisecondOfSecondRange(this ISchedule schedule, int millisecondOfSecondStart, int millisecondOfSecondEnd)
        {
            var millisecondsOfSecond = millisecondOfSecondStart.GetRange(millisecondOfSecondEnd, 1000).ToArray();
            return new MillisecondOfSecondSchedule(schedule, millisecondsOfSecond);
        }
    }
}