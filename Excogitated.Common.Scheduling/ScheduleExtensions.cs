using Excogitated.Common.Extensions;
using System;
using System.Collections.Generic;

namespace Excogitated.Common.Scheduling
{
    public static partial class ScheduleExtensions
    {
        public static DateTimeOffset GetNextEvent(this ISchedule schedule) => schedule.NotNull(nameof(schedule)).GetNextEvent(DateTimeOffset.Now);

        public static DateTimeOffset GetPreviousEvent(this ISchedule schedule) => schedule.NotNull(nameof(schedule)).GetPreviousEvent(DateTimeOffset.Now);

        public static IEnumerable<DateTimeOffset> GetNextEvents(this ISchedule schedule) => schedule.GetNextEvents(DateTimeOffset.Now);

        public static IEnumerable<DateTimeOffset> GetNextEvents(this ISchedule schedule, DateTimeOffset start)
        {
            schedule.NotNull(nameof(schedule));
            var next = start;
            while (true)
            {
                next = schedule.GetNextEvent(next);
                yield return next;
            }
        }

        public static IEnumerable<DateTimeOffset> GetPreviousEvents(this ISchedule schedule) => schedule.GetPreviousEvents(DateTimeOffset.Now);

        public static IEnumerable<DateTimeOffset> GetPreviousEvents(this ISchedule schedule, DateTimeOffset start)
        {
            schedule.NotNull(nameof(schedule));
            var previous = start;
            while (true)
            {
                previous = schedule.GetPreviousEvent(previous);
                yield return previous;
            }
        }

        public static IEnumerable<int> GetRange(this int start, int end, int maxValueExclusive)
        {
            for (var i = start; i != end; i++)
            {
                if (i >= maxValueExclusive)
                    i -= maxValueExclusive;
                yield return i;
            }
            yield return end;
        }
    }

}
