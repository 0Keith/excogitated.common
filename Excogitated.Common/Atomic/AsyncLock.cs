using System;
using System.Threading.Tasks;

namespace Excogitated.Common
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
        private readonly AsyncQueue<int> _locks = new AsyncQueue<int>();
        private readonly IDisposable _exit;

        internal void Exit() => _locks.Add(0);

        public AsyncLock()
        {
            _exit = new AsyncLockExit(this);
            _exit.Dispose();
        }

        public async ValueTask<IDisposable> EnterAsync()
        {
            await _locks.ConsumeAsync();
            return _exit;
        }
    }
}
