using Excogitated.Common.Atomic.Collections;
using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Atomic
{
    internal class AsyncLockExit : IDisposable
    {
        private readonly AsyncLock _asyncLock;

        public void Dispose() => _asyncLock.Exit();

        public AsyncLockExit(AsyncLock asyncLock)
        {
            _asyncLock = asyncLock;
        }
    }

    public class AsyncLock
    {
        private readonly AsyncQueue<int> _locks = new();
        private readonly IDisposable _exit;

        internal void Exit() => _locks.Add(0);

        public AsyncLock()
        {
            _exit = new AsyncLockExit(this);
            _exit.Dispose();
        }

        public async ValueTask<IDisposable> EnterAsync()
        {
            var result = await _locks.TryConsumeAsync();
            if (result.HasValue)
                return _exit;
            throw new Exception("Lock could not be obtained.");
        }
    }
}