using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public static class Benchmark
    {
        public static BenchmarkResult Single(Action action)
        {
            var w = Stopwatch.StartNew();
            action();
            return new BenchmarkResult
            {
                Elapsed = w.Elapsed,
                Executions = 1,
            };
        }

        public static BenchmarkResult<T> Single<T>(Func<T> action)
        {
            var w = Stopwatch.StartNew();
            var value = action();
            return new BenchmarkResult<T>
            {
                Value = value,
                Elapsed = w.Elapsed,
                Executions = 1,
            };
        }

        public static async Task<BenchmarkResult> SingleAsync(Task action)
        {
            var w = Stopwatch.StartNew();
            await action;
            return new BenchmarkResult
            {
                Elapsed = w.Elapsed,
                Executions = 1,
            };
        }

        public static async Task<BenchmarkResult<T>> SingleAsync<T>(Task<T> action)
        {
            var w = Stopwatch.StartNew();
            var value = await action;
            return new BenchmarkResult<T>
            {
                Value = value,
                Elapsed = w.Elapsed,
                Executions = 1,
            };
        }

        public static async ValueTask<BenchmarkResult> SingleAsync(ValueTask action)
        {
            var w = Stopwatch.StartNew();
            await action;
            return new BenchmarkResult
            {
                Elapsed = w.Elapsed,
                Executions = 1,
            };
        }

        public static async ValueTask<BenchmarkResult<T>> SingleAsync<T>(ValueTask<T> action)
        {
            var w = Stopwatch.StartNew();
            var value = await action;
            return new BenchmarkResult<T>
            {
                Value = value,
                Elapsed = w.Elapsed,
                Executions = 1,
            };
        }

        public static ValueTask<BenchmarkResult> Run(Action action) => Run(1, action);
        public static ValueTask<BenchmarkResult> Run(int secondsDuration, Action action) => Run(TimeSpan.FromSeconds(secondsDuration), action);
        public static async ValueTask<BenchmarkResult> Run(TimeSpan duration, Action action)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException("duration <= TimeSpan.Zero");
            await AsyncTimer.Delay(1000); //cooldown cpu
            var d = duration.TotalMilliseconds;
            var threads = Environment.ProcessorCount / 2;
            var results = await Enumerable.Repeat(0, threads).Select(i => Task.Run(() =>
            {
                action(); //warmup code
                var count = 0L;
                var w = Stopwatch.StartNew();
                while (w.ElapsedMilliseconds < d)
                {
                    action();
                    count++;
                }
                return new BenchmarkResult
                {
                    Elapsed = w.Elapsed,
                    Executions = count,
                };
            })).WhenAll();
            return new BenchmarkResult
            {
                Elapsed = TimeSpan.FromMilliseconds(results.Sum(r => r.Elapsed.TotalMilliseconds)),
                Executions = results.Sum(r => r.Executions),
            };
        }

        public static ValueTask<BenchmarkResult> RunAsync(Func<ValueTask> action) => RunAsync(1, action);
        public static ValueTask<BenchmarkResult> RunAsync(int secondsDuration, Func<ValueTask> action) => RunAsync(TimeSpan.FromSeconds(secondsDuration), action);
        public static async ValueTask<BenchmarkResult> RunAsync(TimeSpan duration, Func<ValueTask> action)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException("duration <= TimeSpan.Zero");
            await AsyncTimer.Delay(1000); //cooldown cpu
            var d = duration.TotalMilliseconds;
            var threads = Environment.ProcessorCount / 2;
            var results = await Enumerable.Repeat(0, threads).Select(i => Task.Run(async () =>
            {
                await action(); //warmup code
                var count = 0L;
                var w = Stopwatch.StartNew();
                while (w.ElapsedMilliseconds < d)
                {
                    await action();
                    count++;
                }
                return new BenchmarkResult
                {
                    Elapsed = w.Elapsed,
                    Executions = count,
                };
            })).WhenAll();
            return new BenchmarkResult
            {
                Elapsed = TimeSpan.FromMilliseconds(results.Sum(r => r.Elapsed.TotalMilliseconds)),
                Executions = results.Sum(r => r.Executions),
            };
        }

        public static ValueTask<BenchmarkResult> RunAsync(Func<Task> action) => RunAsync(1, action);
        public static ValueTask<BenchmarkResult> RunAsync(int secondsDuration, Func<Task> action) => RunAsync(TimeSpan.FromSeconds(secondsDuration), action);
        public static async ValueTask<BenchmarkResult> RunAsync(TimeSpan duration, Func<Task> action)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException("duration <= TimeSpan.Zero");
            await AsyncTimer.Delay(1000); //cooldown cpu
            var d = duration.TotalMilliseconds;
            var threads = Environment.ProcessorCount / 2;
            var results = await Enumerable.Repeat(0, threads).Select(i => Task.Run(async () =>
            {
                await action(); //warmup code
                var count = 0L;
                var w = Stopwatch.StartNew();
                while (w.ElapsedMilliseconds < d)
                {
                    await action();
                    count++;
                }
                return new BenchmarkResult
                {
                    Elapsed = w.Elapsed,
                    Executions = count,
                };
            })).WhenAll();
            return new BenchmarkResult
            {
                Elapsed = TimeSpan.FromMilliseconds(results.Sum(r => r.Elapsed.TotalMilliseconds)),
                Executions = results.Sum(r => r.Executions),
            };
        }
    }

    public class BenchmarkResult<T> : BenchmarkResult
    {
        public T Value { get; set; }
    }

    public class BenchmarkResult
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
