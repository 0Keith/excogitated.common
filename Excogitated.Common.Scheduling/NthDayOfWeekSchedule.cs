using System;

namespace Excogitated.Common.Scheduling
{
    internal class NthDayOfWeekSchedule : ISchedule
    {
        private readonly ISchedule _schedule;
        private readonly DayOfWeek _dayOfWeek;
        private readonly int _nthDayOfWeek;

        public NthDayOfWeekSchedule(ISchedule schedule, DayOfWeek dayOfWeek, int nthDayOfWeek)
        {
            if (nthDayOfWeek < 1 || nthDayOfWeek > 5)
                throw new ArgumentException("nthDayOfWeek < 1 || nthDayOfWeek > 5", nameof(nthDayOfWeek));
            _schedule = schedule.OrEvery(TimeUnit.Day);
            _dayOfWeek = dayOfWeek;
            _nthDayOfWeek = nthDayOfWeek;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            while (_dayOfWeek != next.DayOfWeek || Math.Ceiling(next.Day / 7d) != _nthDayOfWeek)
                next = _schedule.GetNextEvent(next);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnDayOfWeek(this ISchedule schedule, DayOfWeek daysOfWeek, int nthDayOfWeek)
        {
            return new NthDayOfWeekSchedule(schedule, daysOfWeek, nthDayOfWeek);
        }
    }
}