using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Excogitated.Common.Execution
{
    internal class MaxRetrySchedule : IScheduledJob
    {
        private readonly IScheduledJob _schedule;
        private readonly int _attempts;

        public MaxRetrySchedule(IScheduledJob schedule, int attempts)
        {
            _schedule = schedule;
            _attempts = attempts;
        }

        public async ValueTask<bool> Execute(ScheduledJobContext context)
        {
            var attempt = 0;
            var exceptions = new List<Exception>();
            while (attempt < _attempts)
                try
                {
                    return await _schedule.Execute(context);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    attempt++;
                }
            throw new AggregateException(exceptions);
        }
    }

    public static partial class ScheduledJobExtensions
    {
        public static IScheduledJob MaxRetries(this IScheduledJob job, int attempts) => new MaxRetrySchedule(job, attempts);
    }
}
