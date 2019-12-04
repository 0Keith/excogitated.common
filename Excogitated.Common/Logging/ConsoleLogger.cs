using System;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public class ConsoleLogger : ILogger, IDisposable
    {
        public static ConsoleLogger CreateDefault(params LogLevel[] levels) => new ConsoleLogger(levels);

        private readonly AsyncQueue<LogMessage> _messages = new AsyncQueue<LogMessage>();
        private readonly bool _all;
        private readonly LogLevel[] _levels;
        private readonly Task _task;
        private bool _running = true;

        public ConsoleLogger(params LogLevel[] levels)
        {
            _all = levels == null || levels.Length == 0 || levels.Contains(LogLevel.All);
            _levels = levels;
            _task = StartLogging();
        }

        public void Dispose()
        {
            _running = false;
            _task.Wait(5000);
        }

        public Task ClearLog()
        {
            _messages.Clear();
            Console.Clear();
            return Task.CompletedTask;
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
                    var result = await _messages.TryConsumeAsync(1000);
                    if (result.HasValue)
                    {
                        await Console.Out.WriteLineAsync(result.Value.Message);
                        await Console.Out.FlushAsync();
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