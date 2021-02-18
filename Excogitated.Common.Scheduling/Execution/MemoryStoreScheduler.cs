using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling.Execution
{
    internal class MemoryStoreScheduler : IScheduledJob
    {
        private readonly ISchedule _schedule;
        private DateTimeOffset _previousEvent;

        public MemoryStoreScheduler(ISchedule schedule)
        {
            _schedule = schedule;
            _previousEvent = _schedule.GetPreviousEvent(DateTimeOffset.Now);
        }

        public ValueTask<DateTimeOffset> GetNextEventAsync()
        {

            return _schedule.GetNextEvent();
        }

        public ValueTask<bool> Execute(ScheduledJobContext context, Func<ScheduledJobContext, ValueTask> executeFunc)
        {
            _previousEvent = context.Expected;
            return executeFunc(context);
        }
    }

    public static partial class ScheduledJobExtensions
    {
        public static IScheduledJob BuildJob(this ISchedule schedule) => new MemoryStoreScheduler(schedule);
    }
}
