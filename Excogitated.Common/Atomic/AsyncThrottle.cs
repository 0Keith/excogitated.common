using Excogitated.Common.Atomic.Collections;
using Excogitated.Common.Logging;
using Excogitated.Threading;
using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Atomic
{
    public class AsyncThrottle : IAsyncDisposable
    {
        private readonly AsyncQueue<bool> _flow = new();
        private readonly AtomicInt32 _consumers = new();
        private readonly AtomicBool _running = new(true);
        private readonly Task _producer;

        public async ValueTask DisposeAsync()
        {
            if (_running.TrySet(false))
            {
                _producer?.Wait();
                while (_consumers > 0)
                {
                    _flow.Add(false);
                    await Task.Delay(10);
                }
                _flow.Clear();
            }
        }

        public AsyncThrottle()
        {
            _producer = Task.CompletedTask;
        }

        public AsyncThrottle(int ratePerSecond = 0)
        {
            if (ratePerSecond > 0)
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

        public void Release(int count = 1)
        {
            for (var i = 0; i < count; i++)
                _flow.Add(true);
        }

        public async ValueTask Wait()
        {
            if (!_running)
                throw new Exception("Throttle is closed.");
            _consumers.Increment();
            var success = await _flow.TryConsumeAsync().LogExecutionTime(false);
            _consumers.Decrement();
            if (!success.HasValue || !success.Value)
                throw new Exception("Throttle was closed.");
        }

        public async ValueTask<bool> TryWait()
        {
            if (!_running)
                return false;
            _consumers.Increment();
            var success = await _flow.TryConsumeAsync().LogExecutionTime(false);
            _consumers.Decrement();
            return success.HasValue && success.Value;
        }
    }
}