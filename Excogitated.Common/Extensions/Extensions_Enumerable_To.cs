using Excogitated.Common.Atomic.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Excogitated.Common.Extensions
{
    public static class Extensions_Enumerable_To
    {
        public static AtomicHashSet<T> ToAtomicHashSet<T>(this IEnumerable<T> source) => new AtomicHashSet<T>(source);
        public static Task<AtomicHashSet<T>> ToAtomicHashSet<T>(this Task<List<T>> source) => source.Continue(s => s.NotNull(nameof(source)).ToAtomicHashSet());
        public static Task<AtomicHashSet<T>> ToAtomicHashSet<T>(this Task<IEnumerable<T>> source) => source.Continue(s => s.NotNull(nameof(source)).ToAtomicHashSet());

        public static AsyncQueue<T> ToAsyncQueue<T>(this IEnumerable<T> source) => new AsyncQueue<T>(source);
        public static AtomicQueue<T> ToAtomicQueue<T>(this IEnumerable<T> source) => new AtomicQueue<T>(source);
    }
}