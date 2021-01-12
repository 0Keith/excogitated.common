using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    internal class HourOfDaySchedule : ISchedule
    {
        private readonly ISchedule _schedule;
        private readonly HashSet<int> _hoursOfDay;

        public HourOfDaySchedule(ISchedule schedule, int[] hoursOfDay)
        {
            if (hoursOfDay is object)
                foreach (var hourOfDay in hoursOfDay)
                    if (hourOfDay < 0 || hourOfDay > 23)
                        throw new ArgumentException("hourOfDay < 0 || hourOfDay > 23", nameof(hourOfDay));
            _schedule = schedule ?? NullSchedule.Instance;
            _hoursOfDay = hoursOfDay?.ToHashSet() ?? new HashSet<int>();
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = GetNextEventPrivate(previousEvent);
            if (_hoursOfDay.Count > 0)
                while (!_hoursOfDay.Contains(next.Hour))
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
        public static ISchedule OnHourOfDay(this ISchedule schedule, params int[] hoursOfDay) => new HourOfDaySchedule(schedule, hoursOfDay);

        public static ISchedule OnHourOfDayRange(this ISchedule schedule, int hourOfDayStart, int hourOfDayEnd)
        {
            var hoursOfDay = hourOfDayStart.GetRange(hourOfDayEnd, 24).ToArray();
            return new HourOfDaySchedule(schedule, hoursOfDay);
        }
    }
}