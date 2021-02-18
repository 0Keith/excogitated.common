using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling.Execution
{
    internal class MaxExecutionSchedule : IScheduledJob
    {
        private readonly IScheduledJob _schedule;
        private readonly int _maxExecutions;
        private int _executionCount;

        public MaxExecutionSchedule(IScheduledJob schedule, int maxExecutions)
        {
            _schedule = schedule;
            _maxExecutions = maxExecutions;
        }

        public ValueTask<DateTimeOffset> GetNextEventAsync(DateTimeOffset previousEvent) => _schedule.GetNextEventAsync(previousEvent);

        public async ValueTask<bool> Execute(ScheduleContext context, Func<ScheduleContext, ValueTask> executeFunc)
        {
            if (_executionCount >= _maxExecutions)
                return false;
            _executionCount++;
            return await _schedule.Execute(context, executeFunc);
        }
    }

    public static partial class ScheduleExtensions
    {
        public static IScheduledJob MaxExecutions(this IScheduledJob schedule, int maxExecutions) => new MaxExecutionSchedule(schedule, maxExecutions);
    }
}
