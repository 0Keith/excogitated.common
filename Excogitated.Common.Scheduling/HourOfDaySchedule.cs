using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    public class HourOfDaySchedule : ISchedule
    {
        private readonly HashSet<int> _hoursOfDay;
        private readonly ISchedule _schedule;

        public HourOfDaySchedule(int[] hoursOfDay, ISchedule schedule = null)
        {
            if (hoursOfDay is object)
                foreach (var hourOfDay in hoursOfDay)
                    if (hourOfDay < 0 || hourOfDay > 23)
                        throw new ArgumentException("hourOfDay < 0 || hourOfDay > 23", nameof(hourOfDay));
            _hoursOfDay = hoursOfDay?.ToHashSet() ?? new HashSet<int>();
            _schedule = schedule ?? NullSchedule.Instance;
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
        public static ISchedule OnHourOfDay(this ISchedule schedule, params int[] hoursOfDay) => new HourOfDaySchedule(hoursOfDay, schedule);

        public static ISchedule OnHourOfDayRange(this ISchedule schedule, int hourOfDayStart, int hourOfDayEnd)
        {
            var hoursOfDay = hourOfDayStart.GetRange(hourOfDayEnd, 24).ToArray();
            return new HourOfDaySchedule(hoursOfDay, schedule);
        }
    }
}