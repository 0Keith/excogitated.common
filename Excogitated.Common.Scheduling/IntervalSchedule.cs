using System;

namespace Excogitated.Common.Scheduling
{
    public class IntervalSchedule : ISchedule
    {
        private readonly TimeUnit _unit;
        private readonly double _interval;
        private readonly ISchedule _schedule;

        public IntervalSchedule(TimeUnit unit, double interval, ISchedule schedule = null)
        {
            _unit = unit;
            _interval = interval;
            _schedule = schedule ?? NullSchedule.Instance;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            return _unit switch
            {
                TimeUnit.Millisecond => next.AddMilliseconds(_interval),
                TimeUnit.Second => next.AddSeconds(_interval),
                TimeUnit.Minute => next.AddMinutes(_interval),
                TimeUnit.Hour => next.AddHours(_interval),
                TimeUnit.Day => next.AddDays(_interval),
                TimeUnit.Month => next.AddMonths((int)_interval),
                TimeUnit.Year => next.AddYears((int)_interval),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule Every(this ISchedule schedule, TimeUnit unit, double interval) => new IntervalSchedule(unit, interval, schedule);
        public static ISchedule EveryMillisecond(this ISchedule schedule, double interval) => schedule.Every(TimeUnit.Millisecond, interval);
        public static ISchedule EverySecond(this ISchedule schedule, double interval) => schedule.Every(TimeUnit.Second, interval);
        public static ISchedule EveryMinute(this ISchedule schedule, double interval) => schedule.Every(TimeUnit.Minute, interval);
        public static ISchedule EveryHour(this ISchedule schedule, double interval) => schedule.Every(TimeUnit.Hour, interval);
        public static ISchedule EveryDay(this ISchedule schedule, double interval) => schedule.Every(TimeUnit.Day, interval);
        public static ISchedule EveryMonth(this ISchedule schedule, int interval) => schedule.Every(TimeUnit.Month, interval);
        public static ISchedule EveryYear(this ISchedule schedule, int interval) => schedule.Every(TimeUnit.Year, interval);
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
