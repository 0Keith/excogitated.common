using System;
using System.IO;
using System.Threading.Tasks;

namespace Excogitated.Common.Scheduling.Execution
{
    internal class FileStoreSchedule : IScheduledJob
    {
        private readonly IScheduledJob _schedule;
        private readonly FileInfo _file;
        private DateTimeOffset? _previousEvent;

        public FileStoreSchedule(IScheduledJob schedule, string fileName)
        {
            _schedule = schedule;
            _file = new FileInfo(fileName);
        }

        public async ValueTask<DateTimeOffset> GetNextEventAsync()
        {
            if (_previousEvent is null && _file.Exists)
            {
                var text = await File.ReadAllTextAsync(_file.FullName);
                if (DateTimeOffset.TryParse(text, out var result))
                    _previousEvent = previousEvent = result;
            }
            return await _schedule.GetNextEventAsync();
        }

        public async ValueTask<bool> Execute(ScheduleContext context, Func<ScheduleContext, ValueTask> executeFunc)
        {
            await File.WriteAllTextAsync(_file.FullName, context.Expected.ToString("O"));
            _previousEvent = context.Expected;
            _file.Refresh();
            return await _schedule.Execute(context, executeFunc);
        }
    }

    public static partial class ScheduleExtensions
    {
        public static IScheduledJob WithFileStore(this IScheduledJob schedule, string fileName) => new FileStoreSchedule(schedule, fileName)
            .ImmediatelyRestart();
    }
}
