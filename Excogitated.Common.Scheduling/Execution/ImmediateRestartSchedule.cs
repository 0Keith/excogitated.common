using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling.Execution
{
    internal class ImmediateRestartSchedule : IScheduledJob
    {
        private readonly IScheduledJob _schedule;

        public ImmediateRestartSchedule(IScheduledJob schedule)
        {
            _schedule = schedule;
        }

        public async ValueTask<DateTimeOffset> GetNextEventAsync(DateTimeOffset previousEvent)
        {
            return await _schedule.GetNextEventAsync(previousEvent);
        }

        public ValueTask<bool> Execute(ScheduleContext context, Func<ScheduleContext, ValueTask> executeFunc)
        {
            return _schedule.Execute(context, executeFunc);
        }
    }

    public static partial class ScheduleExtensions
    {
        public static IScheduledJob ImmediatelyRestart(this IScheduledJob schedule) => new ImmediateRestartSchedule(schedule);
    }
}
