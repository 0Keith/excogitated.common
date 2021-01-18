using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling.Execution
{
    internal class NullScheduleExecutor : IAsyncSchedule
    {
        private readonly ISchedule _schedule;

        public NullScheduleExecutor(ISchedule schedule)
        {
            _schedule = schedule;
        }

        public async Task Execute(DateTimeOffset previousEvent)
        {
            _schedule.GetNextEvent(previousEvent);
        }
    }


    public static partial class ScheduleExtensions
    {
        public static IAsyncSchedule Execute(this ISchedule schedule)
        {
            return new NullScheduleExecutor(schedule);
        }
    }
}
