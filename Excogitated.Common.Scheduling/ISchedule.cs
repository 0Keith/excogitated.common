using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling
{
    public interface ISchedule
    {
        DateTimeOffset GetNextEvent(DateTimeOffset previousEvent);
    }

    public interface IAsyncSchedule
    {
        ValueTask<DateTimeOffset> GetNextEventAsync(DateTimeOffset previousEvent);
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

    public static class Schedule
    {
        public static ISchedule Build() => NullSchedule.Instance;
    }
}
