using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling
{
    internal class ResilientSchedule : IAsyncSchedule
    {
        private readonly IAsyncSchedule _schedule;
        private readonly int _attempts;

        public ResilientSchedule(IAsyncSchedule schedule, int attempts)
        {
            _schedule = schedule;
            _attempts = attempts;
        }

        public DateTimeOffset GetNextEvent(DateTimeOffset previousEvent) => _schedule.GetNextEvent(previousEvent);

        public async ValueTask<bool> Execute(DateTimeOffset nextEvent, Func<DateTimeOffset, ValueTask> executeFunc)
        {
            var attempt = 0;
            var exceptions = new List<Exception>();
            while (attempt < _attempts)
                try
                {
                    await _schedule.Execute(nextEvent, executeFunc);
                    return true;
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
        public static IAsyncSchedule Retry(this IAsyncSchedule schedule, int attempts) => new ResilientSchedule(schedule, attempts);
    }
}
