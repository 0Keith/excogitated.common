using System;
using System.Diagnostics;
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
            var executions = 0;
            var w = Stopwatch.StartNew();
            var d = duration.TotalMilliseconds;
            while (w.ElapsedMilliseconds < d)
            {
                action();
                executions++;
            }
            var elapsed = w.Elapsed;
            return new BenchmarkResult
            {
                Elapsed = elapsed,
                Executions = executions
            };
        }

        public static ValueTask<BenchmarkResult> RunAsync(Func<ValueTask> action) => RunAsync(1, action);
        public static ValueTask<BenchmarkResult> RunAsync(int secondsDuration, Func<ValueTask> action) => RunAsync(TimeSpan.FromSeconds(secondsDuration), action);
        public static async ValueTask<BenchmarkResult> RunAsync(TimeSpan duration, Func<ValueTask> action)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException("duration <= TimeSpan.Zero");
            var executions = 0L;
            var w = Stopwatch.StartNew();
            var d = duration.TotalMilliseconds;
            while (w.ElapsedMilliseconds < d)
            {
                await action();
                executions++;
            }
            var elapsed = w.Elapsed;
            return new BenchmarkResult
            {
                Elapsed = elapsed,
                Executions = executions
            };
        }
    }

    public struct BenchmarkResult
    {
        public TimeSpan Elapsed { get; set; }
        public long Executions { get; set; }
        public TimeSpan Average => TimeSpan.FromSeconds(Elapsed.TotalSeconds / Executions);
        public double PerMillisecond => Executions / Elapsed.TotalMilliseconds;
        public double PerSecond => PerMillisecond * 1000;
        public double PerMinute => PerSecond * 60;
        public double PerHour => PerMinute * 60;
        public double PerDay => PerHour * 24;

        public override string ToString()
        {
            var eps = Math.Round(PerSecond, 2);
            var epms = Math.Round(PerMillisecond, 2);
            return $"Executions: {Executions}, Elapsed: {Elapsed}, Average: {Average}, PerSecond: {eps}, PerMillisecond: {epms}";
        }
    }
}
