using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public class ExecutionTiming
    {
        public AtomicInt64 ElapsedMilliseconds { get; } = new AtomicInt64();
        public AtomicInt32 Count { get; } = new AtomicInt32();
    }

    public static class ExecutionTimings
    {
        private const int maxExecutionTime = 1000;
        private static readonly CowDictionary<string, ExecutionTiming> _timings = new CowDictionary<string, ExecutionTiming>();

        private static void CacheTiming(string name, string file, long elapsedMilliseconds, bool logDelays)
        {
            var key = $"{Path.GetFileNameWithoutExtension(file)}.{name}";
            var timing = _timings.GetOrAdd(key);
            timing.ElapsedMilliseconds.Add(elapsedMilliseconds);
            timing.Count.Increment();
            if (logDelays && elapsedMilliseconds > maxExecutionTime)
                Loggers.Warn($"Execution exceeded {maxExecutionTime}ms. Key: {key}, Elapsed: {elapsedMilliseconds}ms");
        }

        public static Task LogExecutionTime(this Task task, bool logDelays = true, [CallerMemberName] string name = null, [CallerFilePath] string file = null) =>
            new ValueTask(task).LogExecutionTime(logDelays, name, file).AsTask();
        public static async ValueTask LogExecutionTime(this ValueTask task, bool logDelays = true, [CallerMemberName] string name = null, [CallerFilePath] string file = null)
        {
            var w = Stopwatch.StartNew();
            if (task != null)
                await task;
            CacheTiming(name, file, w.ElapsedMilliseconds, logDelays);
        }

        public static Task<T> LogExecutionTime<T>(this Task<T> task, bool logDelays = true, [CallerMemberName] string name = null, [CallerFilePath] string file = null) =>
            new ValueTask<T>(task).LogExecutionTime(logDelays, name, file).AsTask();
        public static async ValueTask<T> LogExecutionTime<T>(this ValueTask<T> task, bool logDelays = true, [CallerMemberName] string name = null, [CallerFilePath] string file = null)
        {
            var w = Stopwatch.StartNew();
            var result = task != null ? await task : default;
            CacheTiming(name, file, w.ElapsedMilliseconds, logDelays);
            return result;
        }

        public static async IAsyncEnumerable<T> LogExecutionTimes<T>(this IAsyncEnumerable<T> source, bool logDelays = true, [CallerMemberName] string name = null, [CallerFilePath] string file = null)
        {
            await using var items = source.GetAsyncEnumerator();
            var w = Stopwatch.StartNew();
            while (await items.MoveNextAsync())
            {
                CacheTiming(name, file, w.ElapsedMilliseconds, logDelays);
                yield return items.Current;
                w.Restart();
            }
        }

        public static async void StartLogging()
        {
            var lastHash = 0;
            using var logger = FileLogger.AppendDefault(typeof(ExecutionTimings));
            while (true)
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(15));
                    if (_timings.Count == 0)
                        continue;
                    var timings = GetTimingsFormatted();
                    var hashCode = timings.GetHashCode();
                    if (hashCode != lastHash)
                    {
                        await logger.ClearLog();
                        logger.Info(timings);
                        lastHash = hashCode;
                    }
                }
                catch (Exception e)
                {
                    Loggers.Error(e);
                }
        }

        public static string GetTimingsFormatted()
        {
            var b = new StringBuilder("***Execution Timings***").AppendLine();
            var timings = _timings.OrderByDescending(t => t.Value.ElapsedMilliseconds.Value).ToList();
            var maxName = timings.MaxOrDefault(t => t.Key.Length);
            var maxCount = timings.MaxOrDefault(t => t.Value.Count.ToString().Length);
            foreach (var t in timings)
            {
                var totalElapsed = TimeSpan.FromMilliseconds(t.Value.ElapsedMilliseconds);
                var avgElapsed = TimeSpan.FromMilliseconds(t.Value.ElapsedMilliseconds / t.Value.Count);
                b.Append($"Name: {t.Key.PadRight(maxName)} ")
                 .Append($"Count: {t.Value.Count.ToString().PadLeft(maxCount)} ")
                 .Append($"Total Elapsed: {totalElapsed} ")
                 .Append($"Avg Elapsed: {avgElapsed} ")
                 .AppendLine();
            }
            return b.ToString();
        }
    }
}
