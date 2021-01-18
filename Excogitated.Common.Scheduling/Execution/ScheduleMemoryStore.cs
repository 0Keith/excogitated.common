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

        public Task Execute(DateTimeOffset previousEvent)
        {
            if (_previousEvent is null)
                _previousEvent = previousEvent;
            return _schedule.Execute(_previousEvent.Value);
        }
    }

    public static partial class ScheduleExtensions
    {
        public static IAsyncSchedule WithMemoryStore(this IAsyncSchedule schedule) => new ScheduleMemoryStore(schedule);
    }
}
