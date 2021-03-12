using System;
using System.Threading;
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

    /// <summary>
    /// Allows locking within an asynchronous method.
    /// This lock can not be reentered from the same thread or async context and attempting to do so will cause a deadlock.
    /// </summary>
    public class AsyncLock
    {
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly IDisposable _exit;

        internal void Exit() => _lock.Release();

        /// <summary>
        /// Create a new AsyncLock.
        /// </summary>
        public AsyncLock()
        {
            _exit = new AsyncLockExit(this);
        }

        /// <summary>
        /// Enter the lock asynchronously. When the returned task completes the lock has been obtained.
        /// The IDisposable result must disposed or the lock will not exit and never be released for another thread to obtain.
        /// This lock can not be reentered from the same thread or async context and attempting to do so will cause a deadlock.
        /// </summary>
        /// <returns>A disposable that releases the lock when disposed. Disposing this object multiple times will cause an exception and destabilizes the lock.</returns>
        public async Task<IDisposable> EnterAsync()
        {
            await _lock.WaitAsync();
            return _exit;
        }
    }
}