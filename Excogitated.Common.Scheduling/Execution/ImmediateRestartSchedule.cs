using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling.Execution
{
    internal class ImmediateRestartSchedule : IAsyncSchedule
    {
        private readonly IAsyncSchedule _schedule;

        public ImmediateRestartSchedule(IAsyncSchedule schedule)
        {
            _schedule = schedule;
        }

        public async ValueTask<DateTimeOffset> GetNextEventAsync(DateTimeOffset previousEvent)
        {
            return await _schedule.GetNextEventAsync(previousEvent);
        }

        public ValueTask<bool> Execute(DateTimeOffset nextEvent, Func<DateTimeOffset, ValueTask> executeFunc)
        {
            return _schedule.Execute(nextEvent, executeFunc);
        }
    }

    public static partial class ScheduleExtensions
    {
        public static IAsyncSchedule ImmediatelyRestart(this IAsyncSchedule schedule) => new ImmediateRestartSchedule(schedule);
    }
}
