using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    public class MillisecondOfSecondSchedule : ISchedule
    {
        private readonly HashSet<int> _millisecondsOfSecond;
        private readonly ISchedule _schedule;

        public MillisecondOfSecondSchedule(int[] millisecondsOfSecond, ISchedule schedule = null)
        {
            if (millisecondsOfSecond is object)
                foreach (var millisecondOfSecond in millisecondsOfSecond)
                    if (millisecondOfSecond < 0 || millisecondOfSecond > 999)
                        throw new ArgumentException("millisecondOfSecond < 0 || millisecondOfSecond > 999", nameof(millisecondOfSecond));
            _millisecondsOfSecond = millisecondsOfSecond?.ToHashSet() ?? new HashSet<int>();
            _schedule = schedule ?? NullSchedule.Instance;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            if (_millisecondsOfSecond.Count > 0)
                while (!_millisecondsOfSecond.Contains(next.Millisecond))
                    next = GetNextEventPrivate(next);
            return next;
        }

        private DateTimeOffset GetNextEventPrivate(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            if (previousEvent == next)
                return next.AddMilliseconds(1);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnMillisecondOfSecond(this ISchedule schedule, params int[] millisecondsOfSecond)
        {
            return new MillisecondOfSecondSchedule(millisecondsOfSecond, schedule);
        }

        public static ISchedule OnMillisecondOfSecondRange(this ISchedule schedule, int millisecondOfSecondStart, int millisecondOfSecondEnd)
        {
            var millisecondsOfSecond = millisecondOfSecondStart.GetRange(millisecondOfSecondEnd, 1000).ToArray();
            return new MillisecondOfSecondSchedule(millisecondsOfSecond, schedule);
        }
    }
}