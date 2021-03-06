﻿using System;

namespace Excogitated.Scheduling
{
    public interface ISchedule
    {
        DateTimeOffset GetNextEvent(DateTimeOffset start);
        DateTimeOffset GetPreviousEvent(DateTimeOffset start);
    }

    public static class Schedule
    {
        public static ISchedule Build() => null;
    }
}
