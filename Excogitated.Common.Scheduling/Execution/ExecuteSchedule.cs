using Excogitated.Common.Extensions;
using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling
{
    internal class ExecuteSchedule
    {
        private readonly ISchedule schedule;
        private readonly Func<ValueTask> executeFunc;

        public ExecuteSchedule(ISchedule schedule, Func<ValueTask> executeFunc)
        {
            this.schedule = schedule;
            this.executeFunc = executeFunc.NotNull(nameof(executeFunc));
        }
    }

    public static partial class ScheduleExtensions
    {
        public static async Task Start(this IAsyncSchedule schedule, Func<ValueTask> executeFunc)
        {
            schedule.NotNull(nameof(schedule));
            executeFunc.NotNull(nameof(schedule));
            var oneMinute = TimeSpan.FromMinutes(1);
            while (true)
            {
                var now = DateTimeOffset.Now;
                await schedule.Execute(now);
                //var timeUntil = next.Subtract(now);
                //if (timeUntil > TimeSpan.Zero)
                //{
                //    if (timeUntil > oneMinute)
                //        timeUntil = oneMinute;
                //    await Task.Delay(timeUntil);
                //}
                //else
                //{
                //    await executeFunc();
                //}
            }
        }
    }
}
