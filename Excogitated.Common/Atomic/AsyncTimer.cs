using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public class DelayResult
    {
        public AsyncResult<int> Result { get; } = new AsyncResult<int>();
        public long Expected { get; }

        public DelayResult(long expected)
        {
            Expected = expected;
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

        public void Dispose() => _running.TrySet(false);

        public AsyncTimerInstance()
        {
            new Thread(NotifyConsumers) { IsBackground = true, Priority = ThreadPriority.Highest }.Start();
        }

        private void NotifyConsumers(object obj)
        {
            while (_running || _consumers.Count > 0)
                try
                {
                    var consumer = GetNextConsumer();
                    if (consumer is null)
                        _mre.WaitOne(60000);
                    else
                    {
                        var delay = Convert.ToInt32(consumer.Expected - _watch.ElapsedMilliseconds);
                        if (delay > 0)
                            _mre.WaitOne(delay);
                        var delta = Convert.ToInt32(_watch.ElapsedMilliseconds - consumer.Expected);
                        if (delta >= 0)
                        {
                            consumer.Result.TryComplete(delta);
                            lock (_consumers)
                                if (_consumers.Last.Value.Is(consumer))
                                    _consumers.RemoveLast();
                        }
                    }
                }
                catch (Exception e)
                {
                    Loggers.Error(e);
                }
        }

        private DelayResult GetNextConsumer()
        {
            lock (_consumers)
                if (_consumers.Count > 0)
                    return _consumers.Last.Value;
            return null;
        }

        public Task<int> Delay(Date date) => Delay(date.DateTime);
        public Task<int> Delay(DateTime date) => Delay(date - DateTime.Now);
        public Task<int> Delay(DateTimeOffset date) => Delay(date - DateTimeOffset.Now);
        public Task<int> Delay(TimeSpan timespan) => Delay((long)timespan.TotalMilliseconds);
        public Task<int> Delay(int milliseconds) => Delay((long)milliseconds);
        public Task<int> Delay(long milliseconds)
        {
            var expected = milliseconds + _watch.ElapsedMilliseconds;
            lock (_consumers)
            {
                var result = new DelayResult(expected);
                if (_consumers.Count == 0)
                {
                    _consumers.AddFirst(result);
                    _mre.Set();
                }
                else
                {
                    var node = _consumers.First;
                    while (node.IsNotNull() && node.Value.Expected > expected)
                        node = node.Next;
                    if (node.IsNotNull())
                        _consumers.AddBefore(node, result);
                    else
                    {
                        _consumers.AddLast(result);
                        _mre.Set();
                    }
                }
                return result.Result.Source;
            }
        }
    }
}