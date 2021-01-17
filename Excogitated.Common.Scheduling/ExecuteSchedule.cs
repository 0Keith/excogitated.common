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
        public static async Task Execute(this ISchedule schedule, Func<ValueTask> executeFunc)
        {

        }
    }
}
