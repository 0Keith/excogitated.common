using System.Threading;

namespace Excogitated.Common
{
    public static class ThreadLocal
    {
        public static ThreadLocal<T> Create<T>(bool trackAllValues = false) where T : new() => new ThreadLocal<T>(() => new T(), trackAllValues);
    }
}
