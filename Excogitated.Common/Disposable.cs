using System;

namespace Excogitated.Common
{
    public class DisposableAction : IDisposable
    {
        private readonly Action _action;

        public void Dispose() => _action?.Invoke();

        public DisposableAction(Action action)
        {
            _action = action;
        }
    }

    public static class Disposable
    {
        public static DisposableAction Action(Action action) => new(action);
    }
}
