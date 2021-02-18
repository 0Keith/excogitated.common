using Excogitated.Common.Extensions;
using System;

namespace Excogitated.Common.Scheduling
{
    internal class GreaterThanSchedule : ISchedule
    {
        private readonly ISchedule _schedule;
        private readonly Func<DateTimeOffset> _greaterThan;

        public GreaterThanSchedule(ISchedule schedule, Func<DateTimeOffset> greaterThan)
        {
            _schedule = schedule.OrEvery(TimeUnit.Day);
            _greaterThan = greaterThan.NotNull(nameof(greaterThan));
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset start)
        {
            var minNext = _greaterThan();
            var next = _schedule.GetNextEvent(start);
            while (next < minNext)
                next = _schedule.GetNextEvent(start);
            return next;
        }

        public DateTimeOffset GetPreviousEvent(DateTimeOffset start)
        {
            var minPrevious = _greaterThan();
            var previous = _schedule.GetPreviousEvent(start);
            while (previous < minPrevious)
                previous = _schedule.GetPreviousEvent(start);
            return previous;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule GreaterThan(this ISchedule schedule, Func<DateTimeOffset> greaterThan) => new GreaterThanSchedule(schedule, greaterThan);

        public static ISchedule GreaterThanNow(this ISchedule schedule) => schedule.GreaterThan(() => DateTimeOffset.Now);
    }
}