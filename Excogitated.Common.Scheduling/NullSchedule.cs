using System;

namespace Excogitated.Common.Scheduling
{
    internal class NullSchedule : ISchedule
    {
        public static NullSchedule Instance { get; } = new NullSchedule();

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent) => previousEvent;
    }
}