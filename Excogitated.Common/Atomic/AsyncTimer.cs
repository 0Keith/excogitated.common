using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public static class AsyncTimer
    {
        private class DelayResult
        {
            public AsyncResult<int> Result { get; } = new AsyncResult<int>();
            public long Expected { get; }

            public DelayResult(long expected)
            {
                Expected = expected;
            }
        }

        private static readonly LinkedList<DelayResult> _consumers = new LinkedList<DelayResult>();
        private static readonly Stopwatch _watch = Stopwatch.StartNew();
        private static readonly Thread _producer = StartProducing();

        private static Thread StartProducing()
        {
            var thread = new Thread(NotifyConsumers);
            if (thread.GetApartmentState() != ApartmentState.MTA)
                thread.SetApartmentState(ApartmentState.MTA);
            thread.Start();
            return thread;
        }

        private static void NotifyConsumers(object obj)
        {
            while (true)
                try
                {
                    var consumer = GetNextConsumer();
                    if (consumer is null)
                        Thread.Sleep(1000);
                    else
                    {
                        var delay = Convert.ToInt32(consumer.Expected - _watch.ElapsedMilliseconds);
                        if (delay > 0)
                            Thread.Sleep(delay);
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
                catch (ThreadInterruptedException) { }
                catch (Exception e)
                {
                    Loggers.Error(e);
                }
        }

        private static DelayResult GetNextConsumer()
        {
            lock (_consumers)
                if (_consumers.Count > 0)
                    return _consumers.Last.Value;
            return null;
        }

        public static Task<int> Delay(int milliseconds)
        {
            var expected = milliseconds + _watch.ElapsedMilliseconds;
            lock (_consumers)
            {
                var result = new DelayResult(expected);
                if (_consumers.Count == 0)
                {
                    _consumers.AddFirst(result);
                    _producer.Interrupt();
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
                        _producer.Interrupt();
                    }
                }
                return result.Result.Source;
            }
        }
    }
}