using System;
using System.Collections.Generic;

namespace Excogitated.Common.Scheduling.Execution
{
    public class ScheduleContext
    {
        public List<DateTimeOffset> MissedEvents { get; } = new List<DateTimeOffset>();
        public DateTimeOffset Expected { get; set; }
    }
}