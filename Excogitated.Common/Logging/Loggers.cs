using Excogitated.Common.Atomic;
using Excogitated.Common.Extensions;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Excogitated.Common.Logging
{
    public enum LogLevel
    {
        All = 0,
        Error = 1,
        Warn = 2,
        Info = 3,
        Debug = 4,
    }

    public interface ILogger
    {
        void Log(LogMessage message);
        Task ClearLog();
    }

    public class LogMessage
    {
        private static readonly AtomicInt32 _id = new();

        public DateTimeOffset Time { get; } = DateTimeOffset.Now;
        public int Id { get; } = _id.Increment();
        public LogLevel Level { get; }
        public string Message { get; }

        public string FormattedMessage => $"{Time:o} | {Level,-5} | {Message}";
        public override string ToString() => FormattedMessage;

        public LogMessage(LogLevel level, object message)
        {
            Level = level;
            Message = message?.ToString() ?? string.Empty;
        }
    }

    public class ModuleLogger : ILogger
    {
        public Task ClearLog() => Module<ILogger>.ResolveAll().Select(l => l.ClearLog()).WhenAll().AsTask();
        public void Log(LogMessage message) => Module<ILogger>.ResolveAll().ForEach(l => l.Log(message));
    }

    public static class Loggers
    {
        public static ILogger All { get; } = new ModuleLogger();

        public static T Register<T>(T logger) where T : ILogger
        {
            Module<ILogger>.Register(logger);
            return logger;
        }

        public static void Warn(object message) => All.Warn(message);
        public static void Error(object message) => All.Error(message);
        public static void Debug(object message) => All.Debug(message);
        public static void Info(object message) => All.Info(message);
    }

    public static class ExtLoggers
    {
        public static void Warn(this ILogger logger, object message) => logger?.Log(new LogMessage(LogLevel.Warn, message));
        public static void Error(this ILogger logger, object message) => logger?.Log(new LogMessage(LogLevel.Error, message));
        public static void Debug(this ILogger logger, object message) => logger?.Log(new LogMessage(LogLevel.Debug, message));
        public static void Info(this ILogger logger, object message) => logger?.Log(new LogMessage(LogLevel.Info, message));

        public static void Started(this ILogger logger, Type type, LogLevel level = LogLevel.Info, [CallerMemberName] string methodName = null)
        {
            var message = $"{nameof(Started)} {type.FullName}.{methodName}";
            logger.Log(new LogMessage(level, message));
        }

        public static void Finished(this ILogger logger, Type type, LogLevel level = LogLevel.Info, [CallerMemberName] string methodName = null)
        {
            var message = $"{nameof(Finished)} {type.FullName}.{methodName}";
            logger.Log(new LogMessage(level, message));
        }
    }
}