using Excogitated.Common.Extensions;
using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling.Execution
{
    public static partial class ScheduleExtensions
    {
        public static async Task Start(this IAsyncSchedule schedule, Func<DateTimeOffset, ValueTask> executeFunc)
        {
            schedule.NotNull(nameof(schedule));
            executeFunc.NotNull(nameof(schedule));
            var oneMinute = TimeSpan.FromMinutes(1);
            var now = DateTimeOffset.Now;
            var today = now.Date;
            var next = await schedule.GetNextEventAsync(today);
            var continueExecution = true;
            while (continueExecution)
            {
                while (next < now)
                    next = await schedule.GetNextEventAsync(next);

                var timeUntil = next.Subtract(now);
                while (timeUntil > TimeSpan.Zero)
                {
                    if (timeUntil > oneMinute)
                        timeUntil = oneMinute;
                    await Task.Delay(timeUntil);
                    now = DateTimeOffset.Now;
                    timeUntil = next.Subtract(now);
                }

                continueExecution = await schedule.Execute(next, executeFunc);
                now = DateTimeOffset.Now;
            }
        }
    }
}
