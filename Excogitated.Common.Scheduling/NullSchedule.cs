using System;

namespace Excogitated.Common.Scheduling
{
    public class NullSchedule : ISchedule
    {
        public static NullSchedule Instance { get; } = new NullSchedule();

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent) => previousEvent;
    }
}