using System;

namespace Excogitated.Scheduling
{
    internal class IntervalSchedule : ISchedule
    {
        private readonly ISchedule _schedule;
        private readonly TimeUnit _unit;
        private readonly double _interval;

        public IntervalSchedule(ISchedule schedule, TimeUnit unit, double interval)
        {
            if (interval == 0)
                throw new ArgumentException("interval == 0");
            _schedule = schedule;
            _unit = unit;
            _interval = interval;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset start)
        {
            var next = _schedule is null ? start : _schedule.GetNextEvent(start);
            return AddTime(next, 1);
        }

        public DateTimeOffset GetPreviousEvent(DateTimeOffset start)
        {
            var previous = _schedule is null ? start : _schedule.GetPreviousEvent(start);
            return AddTime(previous, -1);
        }

        private DateTimeOffset AddTime(DateTimeOffset next, int multiplier)
        {
            var interval = _interval * multiplier;
            return _unit switch
            {
                TimeUnit.Millisecond => next.AddMilliseconds(interval),
                TimeUnit.Second => next.AddSeconds(interval),
                TimeUnit.Minute => next.AddMinutes(interval),
                TimeUnit.Hour => next.AddHours(interval),
                TimeUnit.Day => next.AddDays(interval),
                TimeUnit.Month => next.AddMonths((int)interval),
                TimeUnit.Year => next.AddYears((int)interval),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule Every(this ISchedule schedule, TimeUnit unit, double interval = 1) => new IntervalSchedule(schedule, unit, interval);
        public static ISchedule EveryMillisecond(this ISchedule schedule, double interval = 1) => schedule.Every(TimeUnit.Millisecond, interval);
        public static ISchedule EverySecond(this ISchedule schedule, double interval = 1) => schedule.Every(TimeUnit.Second, interval);
        public static ISchedule EveryMinute(this ISchedule schedule, double interval = 1) => schedule.Every(TimeUnit.Minute, interval);
        public static ISchedule EveryHour(this ISchedule schedule, double interval = 1) => schedule.Every(TimeUnit.Hour, interval);
        public static ISchedule EveryDay(this ISchedule schedule, double interval = 1) => schedule.Every(TimeUnit.Day, interval);
        public static ISchedule EveryMonth(this ISchedule schedule, int interval = 1) => schedule.Every(TimeUnit.Month, interval);
        public static ISchedule EveryYear(this ISchedule schedule, int interval = 1) => schedule.Every(TimeUnit.Year, interval);

        public static ISchedule OrEvery(this ISchedule schedule, TimeUnit unit, double interval = 1)
        {
            return schedule ?? schedule.Every(unit, interval);
        }
    }

    public enum TimeUnit
    {
        Millisecond,
        Second,
        Minute,
        Hour,
        Day,
        Month,
        Year
    }
}
