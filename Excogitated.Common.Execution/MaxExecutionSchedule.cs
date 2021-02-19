using System.Threading.Tasks;

namespace Excogitated.Common.Execution
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

        public async ValueTask<bool> Execute(ScheduledJobContext context)
        {
            if (_executionCount >= _maxExecutions)
                return false;
            _executionCount++;
            return await _schedule.Execute(context);
        }
    }

    public static partial class ScheduledJobExtensions
    {
        public static IScheduledJob MaxExecutions(this IScheduledJob job, int maxExecutions) => new MaxExecutionSchedule(job, maxExecutions);
    }
}
