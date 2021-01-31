using System;
using System.IO;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling.Execution
{
    internal class FileStoreSchedule : IAsyncSchedule
    {
        private readonly IAsyncSchedule _schedule;
        private readonly FileInfo _file;
        private DateTimeOffset? _previousEvent;

        public FileStoreSchedule(IAsyncSchedule schedule, string fileName)
        {
            _schedule = schedule;
            _file = new FileInfo(fileName);
        }

        public async ValueTask<DateTimeOffset> GetNextEventAsync(DateTimeOffset previousEvent)
        {
            if (_previousEvent is null && _file.Exists)
            {
                var text = await File.ReadAllTextAsync(_file.FullName);
                if (DateTimeOffset.TryParse(text, out var result))
                    _previousEvent = result;
            }

            if (_previousEvent != previousEvent)
                await File.WriteAllTextAsync(_file.FullName, previousEvent.ToString("O"));

            return await _schedule.GetNextEventAsync(previousEvent);
        }

        public ValueTask<bool> Execute(DateTimeOffset nextEvent, Func<DateTimeOffset, ValueTask> executeFunc)
        {
            return _schedule.Execute(nextEvent, executeFunc);
        }
    }

    public static partial class ScheduleExtensions
    {
        public static IAsyncSchedule WithFileStore(this IAsyncSchedule schedule, string fileName) => new FileStoreSchedule(schedule, fileName);
    }
}
