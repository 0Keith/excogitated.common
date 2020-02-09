using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public class FileLogger : ILogger, IDisposable
    {
        public static string RootDir { get; set; } = "c:/logs";
        public static FileLogger AppendDefault(Type type, params LogLevel[] levels)
        {
            var levelPath = levels == null || levels.Length == 0 || levels.Contains(LogLevel.All) ? LogLevel.All.ToString() : string.Join(".", levels);
            return new FileLogger(levels, Path.Combine(RootDir, $"{type.FullName}.{levelPath}.log"));
        }

        private readonly AsyncQueue<CollectionTransaction<LogMessage>> _messages = new AsyncQueue<CollectionTransaction<LogMessage>>();
        private readonly AsyncLock _fileLock = new AsyncLock();
        private readonly bool _all;
        private readonly LogLevel[] _levels;
        private readonly bool _formatMessage;
        private readonly Task _task;
        private bool _running = true;

        public FileInfo LogFile { get; }

        public FileLogger(LogLevel[] levels, string fileName, bool formatMessage = true)
        {
            _all = levels == null || levels.Length == 0 || levels.Contains(LogLevel.All);
            _levels = levels;
            _formatMessage = formatMessage;
            LogFile = new FileInfo(fileName);
            if (!LogFile.Directory.Exists)
                LogFile.Directory.Create();
            _task = Task.Run(() => StartLogging());
        }

        public void Dispose()
        {
            _running = false;
            _task.Wait(5000);
        }

        public async Task ClearLog()
        {
            _messages.Clear();
            using (await _fileLock.EnterAsync())
            {
                LogFile.Directory.CreateStrong();
                File.WriteAllText(LogFile.FullName, string.Empty);
            }
        }

        public void Log(LogMessage message)
        {
            if (message != null)
                if (_all || _levels.Contains(message.Level))
                    _messages.Add(message);
        }

        private async Task StartLogging()
        {
            while (_running || _messages.Count > 0)
                try
                {
                    var result = await _messages.PeekAsync(1000);
                    if (result.HasValue)
                        using (await _fileLock.EnterAsync())
                        using (var writer = LogFile.AppendText())
                        using (var messages = _messages.Consume().OrderBy(m => m.Item.Id).ToList().AsDisposable())
                        {
                            foreach (var message in messages)
                                await writer.WriteLineAsync(_formatMessage ? message.Item.FormattedMessage : message.Item.Message);
                            await writer.FlushAsync();
                            foreach (var message in messages)
                                message.Complete();
                        }
                }
                catch (Exception e)
                {
                    await AsyncTimer.Delay(10000);
                    Loggers.Error(e);
                }
        }
    }
}
