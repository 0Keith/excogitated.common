using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling.Execution
{
    internal class MaxRetrySchedule : IAsyncSchedule
    {
        private readonly IAsyncSchedule _schedule;
        private readonly int _attempts;

        public MaxRetrySchedule(IAsyncSchedule schedule, int attempts)
        {
            _schedule = schedule;
            _attempts = attempts;
        }

        public ValueTask<DateTimeOffset> GetNextEventAsync(DateTimeOffset previousEvent) => _schedule.GetNextEventAsync(previousEvent);

        public async ValueTask<bool> Execute(DateTimeOffset nextEvent, Func<DateTimeOffset, ValueTask> executeFunc)
        {
            var attempt = 0;
            var exceptions = new List<Exception>();
            while (attempt < _attempts)
                try
                {
                    return await _schedule.Execute(nextEvent, executeFunc);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    attempt++;
                }
            throw new AggregateException(exceptions);
        }
    }

    public static partial class ScheduleExtensions
    {
        public static IAsyncSchedule MaxRetries(this IAsyncSchedule schedule, int attempts) => new MaxRetrySchedule(schedule, attempts);
    }
}
