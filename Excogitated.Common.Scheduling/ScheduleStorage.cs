using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling
{
    internal class ScheduleMemoryStore : IAsyncSchedule
    {

        public ValueTask<DateTimeOffset> GetNextEventAsync(DateTimeOffset previousEvent)
        {
            throw new NotImplementedException();
        }
    }

    public static partial class ScheduleExtensions
    {
        public static IAsyncSchedule WithMemoryStore(this ISchedule schedule)
        {
            return new ScheduleMemoryStore();
        }
    }
}
