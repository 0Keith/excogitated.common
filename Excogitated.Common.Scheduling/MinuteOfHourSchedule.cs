using System;

namespace Excogitated.Common.Scheduling
{
    public class MinuteOfHourSchedule : ISchedule
    {
        private readonly int _minuteOfHourStart;
        private readonly int _minuteOfHourEnd;
        private readonly ISchedule _schedule;

        public MinuteOfHourSchedule(int minuteOfHourStart, int minuteOfHourEnd, ISchedule schedule = null)
        {
            if (minuteOfHourStart < 0 || minuteOfHourStart > 59)
                throw new ArgumentException("minuteOfHourStart < 0 || minuteOfHourStart > 59", nameof(minuteOfHourStart));
            if (minuteOfHourEnd < 0 || minuteOfHourEnd > 59)
                throw new ArgumentException("minuteOfHourEnd < 0 || minuteOfHourEnd > 59", nameof(minuteOfHourEnd));
            _minuteOfHourStart = minuteOfHourStart;
            _minuteOfHourEnd = minuteOfHourEnd;
            _schedule = schedule ?? NullSchedule.Instance;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            if (_minuteOfHourStart <= _minuteOfHourEnd)
                while (next.Minute < _minuteOfHourStart || next.Minute > _minuteOfHourEnd)
                    next = GetNextEventPrivate(next);
            else
                while (next.Minute < _minuteOfHourStart && next.Minute > _minuteOfHourEnd)
                    next = GetNextEventPrivate(next);
            return next;
        }

        private DateTimeOffset GetNextEventPrivate(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            if (previousEvent == next)
                return next.AddMinutes(1);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnMinuteOfHour(this ISchedule schedule, int minuteOfHourStart, int minuteOfHourEnd)
        {
            return new MinuteOfHourSchedule(minuteOfHourStart, minuteOfHourEnd, schedule);
        }
    }
}