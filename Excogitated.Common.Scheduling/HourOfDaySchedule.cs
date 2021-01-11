using System;

namespace Excogitated.Common.Scheduling
{
    public class HourOfDaySchedule : ISchedule
    {
        private readonly int _hourOfDayStart;
        private readonly int _hourOfDayEnd;
        private readonly ISchedule _schedule;

        public HourOfDaySchedule(int hourOfDayStart, int hourOfDayEnd, ISchedule schedule = null)
        {
            if (hourOfDayStart < 0 || hourOfDayStart > 23)
                throw new ArgumentException("hourOfDayStart < 0 || hourOfDayStart > 23", nameof(hourOfDayStart));
            if (hourOfDayEnd < 0 || hourOfDayEnd > 23)
                throw new ArgumentException("hourOfDayEnd < 0 || hourOfDayEnd > 23", nameof(hourOfDayEnd));
            _hourOfDayStart = hourOfDayStart;
            _hourOfDayEnd = hourOfDayEnd;
            _schedule = schedule ?? NullSchedule.Instance;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            if (_hourOfDayStart <= _hourOfDayEnd)
                while (next.Hour < _hourOfDayStart || next.Hour > _hourOfDayEnd)
                    next = GetNextEventPrivate(next);
            else
                while (next.Hour < _hourOfDayStart && next.Hour > _hourOfDayEnd)
                    next = GetNextEventPrivate(next);
            return next;
        }

        private DateTimeOffset GetNextEventPrivate(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            if (previousEvent == next)
                return next.AddHours(1);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnHourOfDay(this ISchedule schedule, int hourOfDayStart, int hourOfDayEnd)
        {
            return new HourOfDaySchedule(hourOfDayStart, hourOfDayEnd, schedule);
        }
    }
}