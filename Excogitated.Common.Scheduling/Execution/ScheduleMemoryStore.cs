using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling
{
    internal class ScheduleMemoryStore : IAsyncSchedule
    {
        private readonly IAsyncSchedule _schedule;
        private DateTimeOffset? _previousEvent;

        public ScheduleMemoryStore(IAsyncSchedule schedule)
        {
            _schedule = schedule;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            return _schedule.GetNextEvent(previousEvent);
        }

        public ValueTask<bool> Execute(DateTimeOffset nextEvent, Func<DateTimeOffset, ValueTask> executeFunc)
        {
            return _schedule.Execute(nextEvent, executeFunc);
        }
    }

    public static partial class ScheduleExtensions
    {
        public static IAsyncSchedule WithMemoryStore(this IAsyncSchedule schedule) => new ScheduleMemoryStore(schedule);
    }
}
