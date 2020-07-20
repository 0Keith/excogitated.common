using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public class DelayResult
    {
        public AsyncResult<int> Result { get; } = new AsyncResult<int>();
        public double ExpectedElapsed { get; }

        public DelayResult(double expected)
        {
            ExpectedElapsed = expected;
        }
    }

    public static class AsyncTimer
    {
        private static readonly AsyncTimerInstance _instance = new AsyncTimerInstance();

        public static Task<int> Delay(Date date) => _instance.Delay(date);
        public static Task<int> Delay(DateTime date) => _instance.Delay(date);
        public static Task<int> Delay(DateTimeOffset date) => _instance.Delay(date);
        public static Task<int> Delay(TimeSpan timespan) => _instance.Delay(timespan);
        public static Task<int> Delay(int milliseconds) => _instance.Delay(milliseconds);
        public static Task<int> Delay(double milliseconds) => _instance.Delay(milliseconds);
    }

    public class AsyncTimerInstance : IDisposable
    {
        private readonly LinkedList<DelayResult> _consumers = new LinkedList<DelayResult>();
        private readonly ManualResetEventSlim _mre = new ManualResetEventSlim(false);
        private readonly AtomicBool _running = new AtomicBool(true);
        private readonly Stopwatch _watch = Stopwatch.StartNew();
        private double _minExpectedElapsed = double.MaxValue;

        public void Dispose() => _running.TrySet(false);

        public AsyncTimerInstance()
        {
            new Thread(NotifyConsumers) { IsBackground = true }.Start();
        }

        private void NotifyConsumers(object obj)
        {
            var spin = new SpinWait();
            using (_mre)
                while (_running || _consumers.Count > 0)
                    try
                    {
                        var delay = GetDelay();
                        var ready = delay <= 0;
                        if (!ready)
                        {
                            var integerDelay = delay > int.MaxValue ? int.MaxValue : (int)delay;
                            ready = _mre.Wait(integerDelay);
                            if (!ready && delay > integerDelay)
                            {
                                var ticks = (delay - integerDelay) * Stopwatch.Frequency / 1000;
                                var time = Stopwatch.StartNew();
                                while (time.ElapsedTicks < ticks)
                                    spin.SpinOnce();
                            }
                        }

                        if (ready)
                        {
                            lock (_consumers)
                            {
                                _mre.Reset();
                                var elapsed = _watch.ElapsedMilliseconds;
                                var next = _consumers.First;
                                while (next != null)
                                {
                                    if (next.Value.ExpectedElapsed <= elapsed)
                                    {
                                        var result = elapsed - next.Value.ExpectedElapsed;
                                        next.Value.Result.TryComplete((int)result);
                                        _consumers.Remove(next);
                                    }
                                    next = next.Next;
                                }
                                _minExpectedElapsed = _consumers.Count == 0 ? long.MaxValue : _consumers.Min(c => c.ExpectedElapsed);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Loggers.Error(e);
                    }
        }

        private double GetDelay()
        {
            lock (_consumers)
            {
                return _minExpectedElapsed - _watch.ElapsedMilliseconds;
            }
        }

        public Task<int> Delay(Date date) => Delay(date.DateTime);
        public Task<int> Delay(DateTime date) => Delay(date - DateTime.Now);
        public Task<int> Delay(DateTimeOffset date) => Delay(date - DateTimeOffset.Now);
        public Task<int> Delay(TimeSpan timespan) => Delay(timespan.TotalMilliseconds);
        public Task<int> Delay(int milliseconds) => Delay((double)milliseconds);
        public Task<int> Delay(double milliseconds)
        {
            var expectedElapsed = milliseconds + _watch.ElapsedMilliseconds;
            var result = new DelayResult(expectedElapsed);
            lock (_consumers)
            {
                _consumers.AddLast(result);
                if (expectedElapsed < _minExpectedElapsed)
                {
                    _minExpectedElapsed = expectedElapsed;
                    _mre.Set();
                }
                return result.Result.Source;
            }
        }
    }
}