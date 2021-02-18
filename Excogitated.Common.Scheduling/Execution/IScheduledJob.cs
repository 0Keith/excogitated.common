using Excogitated.Common.Extensions;
using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling.Execution
{
    public interface IScheduledJob
    {
        ValueTask<bool> Execute(ScheduledJobContext context);
    }

    public static partial class ScheduledJobExtensions
    {
        public static async Task Start(this IScheduledJob schedule, Func<ScheduledJobContext, ValueTask> executeFunc)
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
                var context = new ScheduledJobContext();
                while (next < now)
                {
                    context.MissedEvents.Add(next);
                    next = await schedule.GetNextEventAsync(next);
                }

                var timeUntil = next.Subtract(now);
                while (timeUntil > TimeSpan.Zero)
                {
                    if (timeUntil > oneMinute)
                        timeUntil = oneMinute;
                    await Task.Delay(timeUntil);
                    now = DateTimeOffset.Now;
                    timeUntil = next.Subtract(now);
                }

                continueExecution = await schedule.Execute(context);
                now = DateTimeOffset.Now;
            }
        }
    }
}
