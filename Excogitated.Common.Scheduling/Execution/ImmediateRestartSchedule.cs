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

        public ValueTask<bool> Execute(ScheduledJobContext context, Func<ScheduledJobContext, ValueTask> executeFunc)
        {
            return _schedule.Execute(context, executeFunc);
        }
    }

    public static partial class ScheduledJobExtensions
    {
        public static IScheduledJob ImmediatelyRestart(this IScheduledJob job) => new ImmediateRestartSchedule(job);
    }
}
