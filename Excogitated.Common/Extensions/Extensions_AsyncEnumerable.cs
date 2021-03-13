using Excogitated.Common.Atomic;
using Excogitated.Extensions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Excogitated.Common.Extensions
{
    public static class Extensions_AsyncEnumerable
    {
        public static async IAsyncEnumerable<T> Watch<T>(this IAsyncEnumerable<T> source, int estimatedTotal = 0, [CallerMemberName] string name = null)
        {
            source.NotNull(nameof(source));
            using var _watch = new AtomicWatch(estimatedTotal, name);
            await foreach (var item in source)
            {
                yield return item;
                _watch.Increment();
            }
        }
    }
}