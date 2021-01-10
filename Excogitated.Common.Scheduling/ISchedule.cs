using System;
using System.Collections.Generic;

namespace Excogitated.Common.Scheduling
{
    public interface ISchedule
    {
        DateTimeOffset GetNextEvent(DateTimeOffset previousEvent);
    }

    public static partial class ScheduleExtensions
    {
        public static IEnumerable<DateTimeOffset> GetEvents(this ISchedule schedule, DateTimeOffset? previousEvent = null)
        {
            var next = previousEvent ?? DateTimeOffset.Now;
            while (true)
            {
                next = schedule.GetNextEvent(next);
                yield return next;
            }
        }
    }

    public static class Schedule
    {
        public static ISchedule Build() => NullSchedule.Instance;
    }
}
