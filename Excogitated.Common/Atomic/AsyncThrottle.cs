using System;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public class AsyncThrottle : IDisposable
    {
        private readonly AsyncQueue<bool> _flow = new AsyncQueue<bool>();
        private readonly AtomicInt32 _consumers = new AtomicInt32();
        private readonly AtomicBool _running = new AtomicBool(true);
        private readonly Task _producer;

        public void Dispose()
        {
            if (_running.TrySet(false))
            {
                _producer.Wait();
                while (_consumers > 0)
                {
                    _flow.Add(false);
                    Thread.Sleep(10);
                }
                _flow.Clear();
            }
        }

        public AsyncThrottle()
        {
            _producer = Task.CompletedTask;
        }

        public AsyncThrottle(int ratePerSecond)
        {
            if (ratePerSecond < 1)
                ratePerSecond = 1;
            _producer = Task.Run(async () =>
            {
                while (_running)
                {
                    await Task.Delay(1000);
                    var count = ratePerSecond - _flow.Count;
                    for (var i = 0; i < count; i++)
                        _flow.Add(true);
                }
            });
        }

        public void Release(int count)
        {
            for (var i = 0; i < count; i++)
                _flow.Add(true);
        }

        public async ValueTask Wait()
        {
            if (!_running)
                throw new Exception("Throttle is closed.");
            _consumers.Increment();
            var success = await _flow.ConsumeAsync().LogExecutionTime();
            _consumers.Decrement();
            if (!success.HasValue || !success.Value)
                throw new Exception("Throttle was closed.");
        }
    }
}