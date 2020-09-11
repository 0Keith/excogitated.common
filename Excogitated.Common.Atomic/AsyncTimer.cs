using Excogitated.Common.Atomic.Collections;
using Excogitated.Common.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.Common.Atomic
{
    public class DelayResult
    {
        public AsyncResult<TimeSpan> Result { get; } = new AsyncResult<TimeSpan>();
        public DateTimeOffset Expected { get; }

        public DelayResult(DateTimeOffset expected)
        {
            Expected = expected;
        }
    }

    public static class AsyncTimer
    {
        private static readonly AsyncTimerInstance _instance = new AsyncTimerInstance();

        public static Task<TimeSpan> Delay(Date date) => _instance.Delay(date);
        public static Task<TimeSpan> Delay(DateTime date) => _instance.Delay(date);
        public static Task<TimeSpan> Delay(TimeSpan timespan) => _instance.Delay(timespan);
        public static Task<TimeSpan> Delay(int milliseconds) => _instance.Delay(milliseconds);
        public static Task<TimeSpan> Delay(double milliseconds) => _instance.Delay(milliseconds);
        public static Task<TimeSpan> Delay(DateTimeOffset date) => _instance.Delay(date);
    }

    public class AsyncTimerInstance : IDisposable
    {
        private static readonly long _ticksPerMillisecond = Stopwatch.Frequency / 1000;

        private readonly LinkedList<DelayResult> _consumers = new LinkedList<DelayResult>();
        private readonly ManualResetEventSlim _mre = new ManualResetEventSlim(false);
        private readonly AtomicBool _running = new AtomicBool(true);

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
                        var date = GetNextDate();
                        var now = DateTimeOffset.Now;
                        var ready = now >= date;
                        if (!ready)
                        {
                            var delay = (date - now).TotalMilliseconds;
                            var integerDelay = delay > int.MaxValue ? int.MaxValue : (int)delay;
                            ready = _mre.Wait(integerDelay);
                            if (!ready && delay > integerDelay)
                            {
                                var ticks = (delay - integerDelay) * _ticksPerMillisecond;
                                if (ticks <= _ticksPerMillisecond)
                                {
                                    var time = Stopwatch.StartNew();
                                    while (time.ElapsedTicks < ticks)
                                        spin.SpinOnce();
                                    ready = true;
                                }
                            }
                        }

                        if (ready)
                        {
                            lock (_consumers)
                            {
                                _mre.Reset();
                                now = DateTimeOffset.Now;
                                var node = _consumers.First;
                                while (node != null && node.Value.Expected <= now)
                                {
                                    node.Value.Result.TryComplete(now - node.Value.Expected);
                                    var prev = node;
                                    node = node.Next;
                                    _consumers.Remove(prev);
                                    now = DateTimeOffset.Now;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Loggers.Error(e);
                    }
        }

        private DateTimeOffset GetNextDate()
        {
            lock (_consumers)
            {
                return _consumers.First?.Value.Expected ?? DateTimeOffset.MaxValue;
            }
        }

        public Task<TimeSpan> Delay(Date date) => Delay(date.DateTime);
        public Task<TimeSpan> Delay(DateTime date) => Delay(new DateTimeOffset(date));
        public Task<TimeSpan> Delay(TimeSpan timespan) => Delay(DateTimeOffset.Now.Add(timespan));
        public Task<TimeSpan> Delay(int milliseconds) => Delay(DateTimeOffset.Now.AddMilliseconds(milliseconds));
        public Task<TimeSpan> Delay(double milliseconds) => Delay(DateTimeOffset.Now.AddMilliseconds(milliseconds));
        public Task<TimeSpan> Delay(DateTimeOffset date)
        {
            var result = new DelayResult(date);
            lock (_consumers)
            {
                var node = _consumers.First;
                if (node is null)
                    _consumers.AddFirst(result);
                else
                {
                    while (node is object)
                    {
                        if (node.Value.Expected >= date)
                        {
                            _consumers.AddBefore(node, result);
                            break;
                        }
                        node = node.Next;
                    }
                    if (node is null)
                        _consumers.AddLast(result);
                }
                _mre.Set();
                return result.Result.Source;
            }
        }
    }
}