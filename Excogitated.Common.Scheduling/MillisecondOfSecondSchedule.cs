using System;

namespace Excogitated.Common.Scheduling
{
    public class MillisecondOfSecondSchedule : ISchedule
    {
        private readonly int _millisecondOfSecondStart;
        private readonly int _millisecondOfSecondEnd;
        private readonly ISchedule _schedule;

        public MillisecondOfSecondSchedule(int millisecondOfSecondStart, int millisecondOfSecondEnd, ISchedule schedule = null)
        {
            if (millisecondOfSecondStart < 0 || millisecondOfSecondStart > 999)
                throw new ArgumentException("millisecondOfSecondStart < 0 || millisecondOfSecondStart > 999", nameof(millisecondOfSecondStart));
            if (millisecondOfSecondEnd < 0 || millisecondOfSecondEnd > 999)
                throw new ArgumentException("millisecondOfSecondEnd < 0 || millisecondOfSecondEnd > 999", nameof(millisecondOfSecondEnd));
            _millisecondOfSecondStart = millisecondOfSecondStart;
            _millisecondOfSecondEnd = millisecondOfSecondEnd;
            _schedule = schedule ?? NullSchedule.Instance;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            if (_millisecondOfSecondStart <= _millisecondOfSecondEnd)
                while (next.Millisecond < _millisecondOfSecondStart || next.Millisecond > _millisecondOfSecondEnd)
                    next = GetNextEventPrivate(next);
            else
                while (next.Millisecond < _millisecondOfSecondStart && next.Millisecond > _millisecondOfSecondEnd)
                    next = GetNextEventPrivate(next);
            return next;
        }

        private DateTimeOffset GetNextEventPrivate(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            if (previousEvent == next)
                return next.AddMilliseconds(1);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnMillisecondOfSecond(this ISchedule schedule, int millisecondOfSecondStart, int millisecondOfSecondEnd)
        {
            return new MillisecondOfSecondSchedule(millisecondOfSecondStart, millisecondOfSecondEnd, schedule);
        }
    }
}