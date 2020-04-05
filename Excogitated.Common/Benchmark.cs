using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public static class Benchmark
    {
        public static BenchmarkResult Run(Action action) => Run(1, action);
        public static BenchmarkResult Run(int secondsDuration, Action action) => Run(TimeSpan.FromSeconds(secondsDuration), action);
        public static BenchmarkResult Run(TimeSpan duration, Action action)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException("duration <= TimeSpan.Zero");
            var w = Stopwatch.StartNew();
            var d = duration.TotalMilliseconds;
            var threads = Environment.ProcessorCount / 2;
            var executions = Task.WhenAll(Enumerable.Repeat(0, threads).Select(i => Task.Run(() =>
            {
                var count = 0L;
                while (w.ElapsedMilliseconds < d)
                {
                    action();
                    count++;
                }
                return count;
            }))).Result.Sum();
            return new BenchmarkResult
            {
                Elapsed = w.Elapsed,
                Executions = executions
            };
        }

        public static ValueTask<BenchmarkResult> RunAsync(Func<ValueTask> action) => RunAsync(1, action);
        public static ValueTask<BenchmarkResult> RunAsync(int secondsDuration, Func<ValueTask> action) => RunAsync(TimeSpan.FromSeconds(secondsDuration), action);
        public static async ValueTask<BenchmarkResult> RunAsync(TimeSpan duration, Func<ValueTask> action)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException("duration <= TimeSpan.Zero");
            var w = Stopwatch.StartNew();
            var d = duration.TotalMilliseconds;
            var threads = Environment.ProcessorCount / 2;
            var executions = await Task.WhenAll(Enumerable.Repeat(0, threads).Select(i => Task.Run(async () =>
            {
                var count = 0L;
                while (w.ElapsedMilliseconds < d)
                {
                    await action();
                    count++;
                }
                return count;
            })));
            return new BenchmarkResult
            {
                Elapsed = w.Elapsed,
                Executions = executions.Sum()
            };
        }

        public static Task<BenchmarkResult> RunAsync(Func<Task> action) => RunAsync(1, action);
        public static Task<BenchmarkResult> RunAsync(int secondsDuration, Func<Task> action) => RunAsync(TimeSpan.FromSeconds(secondsDuration), action);
        public static async Task<BenchmarkResult> RunAsync(TimeSpan duration, Func<Task> action)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException("duration <= TimeSpan.Zero");
            var w = Stopwatch.StartNew();
            var d = duration.TotalMilliseconds;
            var threads = Environment.ProcessorCount / 2;
            var executions = await Task.WhenAll(Enumerable.Repeat(0, threads).Select(i => Task.Run(async () =>
            {
                var count = 0L;
                while (w.ElapsedMilliseconds < d)
                {
                    await action();
                    count++;
                }
                return count;
            })));
            return new BenchmarkResult
            {
                Elapsed = w.Elapsed,
                Executions = executions.Sum()
            };
        }
    }

    public struct BenchmarkResult
    {
        public TimeSpan Elapsed { get; set; }
        public long Executions { get; set; }
        public TimeSpan Average => GetAverage();
        public double PerMillisecond => Executions / Elapsed.TotalMilliseconds;
        public double PerSecond => PerMillisecond * 1000;
        public double PerMinute => PerSecond * 60;
        public double PerHour => PerMinute * 60;
        public double PerDay => PerHour * 24;

        public TimeSpan GetAverage()
        {
            var avg = Elapsed.TotalSeconds / Executions;
            if (avg > TimeSpan.MaxValue.TotalSeconds)
                return TimeSpan.MaxValue;
            if (avg < TimeSpan.MinValue.TotalSeconds)
                return TimeSpan.MinValue;
            return TimeSpan.FromSeconds(avg);
        }

        public override string ToString()
        {
            var eps = Math.Round(PerSecond, 2);
            var epms = Math.Round(PerMillisecond, 2);
            return $"Executions: {Executions}, Elapsed: {Elapsed}, Average: {Average}, PerSecond: {eps}, PerMillisecond: {epms}";
        }
    }
}
