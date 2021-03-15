using System;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.Threading
{
    internal class AsyncLimiterRelease : IDisposable
    {
        private readonly AsyncLimiter _throttle;

        public void Dispose() => _throttle.ReleaseThread();

        public AsyncLimiterRelease(AsyncLimiter throttle)
        {
            _throttle = throttle;
        }
    }

    public class AsyncLimiter

    {
        private readonly AtomicBool _running = new(true);
        private readonly SemaphoreSlim _rateLimit;
        private readonly SemaphoreSlim _threadLimit;
        private readonly AsyncLimiterRelease _release;

        ~AsyncLimiter() => _running.Value = false;

        public AsyncLimiter(int maxConcurrency = 0, int maxRate = 0, TimeSpan rateInterval = default)
        {
            _release = new(this);
            if (maxConcurrency > 0)
                _threadLimit = new(maxConcurrency, maxConcurrency);
            if (maxRate > 0 && rateInterval > TimeSpan.Zero)
            {
                _rateLimit = new(0);
                var interval = TimeSpan.FromMilliseconds(rateInterval.TotalMilliseconds / maxRate);
                StartRateLimitReleaser(interval);
            }
        }

        private async void StartRateLimitReleaser(TimeSpan releaseInterval)
        {
            while (_running)
            {
                await Task.Delay(releaseInterval);
                _rateLimit.Release();
            }
        }

        internal void ReleaseThread()
        {
            if (_threadLimit is not null)
                _threadLimit.Release();
        }

        public async ValueTask<IDisposable> WaitAsync()
        {
            if (_threadLimit is not null)
                await _threadLimit.WaitAsync();
            if (_rateLimit is not null)
                await _rateLimit.WaitAsync();
            return _release;
        }
    }
}