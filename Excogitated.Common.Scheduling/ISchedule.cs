using Excogitated.Common.Scheduling.Execution;
using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling
{
    public interface ISchedule
    {
        DateTimeOffset GetNextEvent(DateTimeOffset start);
        DateTimeOffset GetPreviousEvent(DateTimeOffset start);
    }

    public interface IScheduledJob
    {
        ValueTask<bool> Execute(ScheduleContext context, Func<ScheduleContext, ValueTask> executeFunc);
    }

    public static class Schedule
    {
        public static ISchedule Build() => null;
    }
}
