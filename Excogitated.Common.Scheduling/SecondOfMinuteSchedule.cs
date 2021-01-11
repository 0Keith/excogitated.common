using System;

namespace Excogitated.Common.Scheduling
{
    public class SecondOfMinuteSchedule : ISchedule
    {
        private readonly int _secondOfMinuteStart;
        private readonly int _secondOfMinuteEnd;
        private readonly ISchedule _schedule;

        public SecondOfMinuteSchedule(int secondOfMinuteStart, int secondOfMinuteEnd, ISchedule schedule = null)
        {
            if (secondOfMinuteStart < 0 || secondOfMinuteStart > 59)
                throw new ArgumentException("secondOfMinuteStart < 0 || secondOfMinuteStart > 59", nameof(secondOfMinuteStart));
            if (secondOfMinuteEnd < 0 || secondOfMinuteEnd > 59)
                throw new ArgumentException("secondOfMinuteEnd < 0 || secondOfMinuteEnd > 59", nameof(secondOfMinuteEnd));
            _secondOfMinuteStart = secondOfMinuteStart;
            _secondOfMinuteEnd = secondOfMinuteEnd;
            _schedule = schedule ?? NullSchedule.Instance;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            if (_secondOfMinuteStart <= _secondOfMinuteEnd)
                while (next.Second < _secondOfMinuteStart || next.Second > _secondOfMinuteEnd)
                    next = GetNextEventPrivate(next);
            else
                while (next.Second < _secondOfMinuteStart && next.Second > _secondOfMinuteEnd)
                    next = GetNextEventPrivate(next);
            return next;
        }

        private DateTimeOffset GetNextEventPrivate(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            if (previousEvent == next)
                return next.AddSeconds(1);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnSecondOfMinute(this ISchedule schedule, int secondOfMinuteStart, int secondOfMinuteEnd)
        {
            return new SecondOfMinuteSchedule(secondOfMinuteStart, secondOfMinuteEnd, schedule);
        }
    }
}