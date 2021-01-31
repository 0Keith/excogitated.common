using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling.Execution
{
    internal class NullAsyncSchedule : IAsyncSchedule
    {
        private readonly ISchedule _schedule;

        public NullAsyncSchedule(ISchedule schedule)
        {
            _schedule = schedule;
        }

        public ValueTask<DateTimeOffset> GetNextEventAsync(DateTimeOffset previousEvent) => ValueTask.FromResult(_schedule.GetNextEvent(previousEvent));

        public async ValueTask<bool> Execute(DateTimeOffset nextEvent, Func<DateTimeOffset, ValueTask> executeFunc)
        {
            await executeFunc(nextEvent);
            return true;
        }
    }

    public static partial class ScheduleExtensions
    {
        public static IAsyncSchedule Execute(this ISchedule schedule)
        {
            return new NullAsyncSchedule(schedule);
        }
    }
}
