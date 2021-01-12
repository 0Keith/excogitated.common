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
            _schedule = schedule ?? NullSchedule.Instance;
            _dayOfWeek = dayOfWeek;
            _nthDayOfWeek = nthDayOfWeek;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            while (_dayOfWeek != next.DayOfWeek || next.Day / 7 != _nthDayOfWeek)
                next = GetNextEventPrivate(next);
            return next;
        }

        private DateTimeOffset GetNextEventPrivate(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            if (previousEvent == next)
                return next.AddDays(1);
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
