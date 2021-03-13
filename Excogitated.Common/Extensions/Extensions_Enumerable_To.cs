using Excogitated.Common.Atomic.Collections;
using Excogitated.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Excogitated.Common.Extensions
{
    public static class Extensions_Enumerable_To
    {
        public static AtomicHashSet<T> ToAtomicHashSet<T>(this IEnumerable<T> source) => new(source);
        public static Task<AtomicHashSet<T>> ToAtomicHashSet<T>(this Task<List<T>> source) => source.Continue(s => s.NotNull(nameof(source)).ToAtomicHashSet());
        public static Task<AtomicHashSet<T>> ToAtomicHashSet<T>(this Task<IEnumerable<T>> source) => source.Continue(s => s.NotNull(nameof(source)).ToAtomicHashSet());

        public static AsyncQueue<T> ToAsyncQueue<T>(this IEnumerable<T> source) => new(source);
        public static AtomicQueue<T> ToAtomicQueue<T>(this IEnumerable<T> source) => new(source);
    }
}