using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling
{
    internal class MaxExecutionSchedule : IAsyncSchedule
    {
        private readonly IAsyncSchedule _schedule;
        private readonly int _maxExecutions;
        private int _executionCount;

        public MaxExecutionSchedule(IAsyncSchedule schedule, int maxExecutions)
        {
            _schedule = schedule;
            _maxExecutions = maxExecutions;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent)
        {
            return _schedule.GetNextEvent(previousEvent);
        }

        public async ValueTask<bool> Execute(DateTimeOffset nextEvent, Func<DateTimeOffset, ValueTask> executeFunc)
        {
            if (_executionCount >= _maxExecutions)
                return false;
            _executionCount++;
            return await _schedule.Execute(nextEvent, executeFunc);
        }
    }

    public static partial class ScheduleExtensions
    {
        public static IAsyncSchedule MaxExecutions(this IAsyncSchedule schedule, int maxExecutions) => new MaxExecutionSchedule(schedule, maxExecutions);
    }
}
