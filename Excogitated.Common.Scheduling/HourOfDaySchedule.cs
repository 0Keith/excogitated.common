using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Scheduling
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
            _schedule = schedule.OrEvery(TimeUnit.Hour);
            _hoursOfDay = hoursOfDay?.ToHashSet() ?? new HashSet<int>();
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset start)
        {
            var next = _schedule.GetNextEvent(start);
            if (_hoursOfDay.Count > 0)
                while (!_hoursOfDay.Contains(next.Hour))
                    next = _schedule.GetNextEvent(next);
            return next;
        }

        public DateTimeOffset GetPreviousEvent(DateTimeOffset start)
        {
            var previous = _schedule.GetPreviousEvent(start);
            if (_hoursOfDay.Count > 0)
                while (!_hoursOfDay.Contains(previous.Hour))
                    previous = _schedule.GetPreviousEvent(previous);
            return previous;
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