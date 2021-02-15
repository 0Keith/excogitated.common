﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Scheduling
{
    internal class MinuteOfHourSchedule : ISchedule
    {
        private readonly ISchedule _schedule;
        private readonly HashSet<int> _minutesOfHour;

        public MinuteOfHourSchedule(ISchedule schedule, int[] minutesOfHour)
        {
            if (minutesOfHour is object)
                foreach (var minuteOfHour in minutesOfHour)
                    if (minuteOfHour < 0 || minuteOfHour > 59)
                        throw new ArgumentException("minuteOfHour < 0 || minuteOfHour > 59", nameof(minuteOfHour));
            _schedule = schedule.OrEvery(TimeUnit.Minute);
            _minutesOfHour = minutesOfHour?.ToHashSet() ?? new HashSet<int>();
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            var next = _schedule.GetNextEvent(previousEvent);
            if (_minutesOfHour.Count > 0)
                while (!_minutesOfHour.Contains(next.Minute))
                    next = _schedule.GetNextEvent(next);
            return next;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static ISchedule OnMinuteOfHour(this ISchedule schedule, params int[] minutesOfHour) => new MinuteOfHourSchedule(schedule, minutesOfHour);

        public static ISchedule OnMinuteOfHourRange(this ISchedule schedule, int minuteOfHourStart, int minuteOfHourEnd)
        {
            var minutesOfHour = minuteOfHourStart.GetRange(minuteOfHourEnd, 60).ToArray();
            return new MinuteOfHourSchedule(schedule, minutesOfHour);
        }
    }
}