using System.Threading;

namespace Excogitated.Common.Extensions
{
    public static class ThreadLocal
    {
        public static ThreadLocal<T> Create<T>(bool trackAllValues = false) where T : new() => new ThreadLocal<T>(() => new T(), trackAllValues);
    }
}
