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
        public long ExpectedElapsed { get; }

        public DelayResult(long expected)
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
        public static Task<int> Delay(long milliseconds) => _instance.Delay(milliseconds);
    }

    public class AsyncTimerInstance : IDisposable
    {
        private readonly LinkedList<DelayResult> _consumers = new LinkedList<DelayResult>();
        private readonly ManualResetEvent _mre = new ManualResetEvent(false);
        private readonly AtomicBool _running = new AtomicBool(true);
        private readonly Stopwatch _watch = Stopwatch.StartNew();
        private long _minExpectedElapsed = long.MaxValue;

        public void Dispose() => _running.TrySet(false);

        public AsyncTimerInstance()
        {
            new Thread(NotifyConsumers) { IsBackground = true }.Start();
        }

        private void NotifyConsumers(object obj)
        {
            using (_mre)
                while (_running || _consumers.Count > 0)
                    try
                    {
                        var delay = GetDelay();
                        if (delay <= 0 || _mre.WaitOne(delay))
                            lock (_consumers)
                            {
                                _mre.Reset();
                                var elapsed = _watch.ElapsedMilliseconds;
                                var next = _consumers.First;
                                while (next != null)
                                {
                                    if (next.Value.ExpectedElapsed <= elapsed)
                                    {
                                        next.Value.Result.TryComplete((elapsed - next.Value.ExpectedElapsed).ToInt());
                                        _consumers.Remove(next);
                                    }
                                    next = next.Next;
                                }
                                _minExpectedElapsed = _consumers.Count == 0 ? long.MaxValue : _consumers.Min(c => c.ExpectedElapsed);
                            }
                    }
                    catch (Exception e)
                    {
                        Loggers.Error(e);
                    }
        }

        private int GetDelay()
        {
            lock (_consumers)
            {
                return (_minExpectedElapsed - _watch.ElapsedMilliseconds).ToInt();
            }
        }

        public Task<int> Delay(Date date) => Delay(date.DateTime);
        public Task<int> Delay(DateTime date) => Delay(date - DateTime.Now);
        public Task<int> Delay(DateTimeOffset date) => Delay(date - DateTimeOffset.Now);
        public Task<int> Delay(TimeSpan timespan) => Delay((long)timespan.TotalMilliseconds);
        public Task<int> Delay(int milliseconds) => Delay((long)milliseconds);
        public Task<int> Delay(long milliseconds)
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