using System.Threading;

namespace Excogitated.Extensions
{
    public static class ThreadLocal
    {
        public static ThreadLocal<T> Create<T>(bool trackAllValues = false) where T : new() => new(() => new T(), trackAllValues);
    }
}
