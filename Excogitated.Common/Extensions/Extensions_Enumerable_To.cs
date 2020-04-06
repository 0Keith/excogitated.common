using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public static class Extensions_Enumerable_To
    {
#if NETSTANDARD2_0
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) => new HashSet<T>(source.NotNull(nameof(source)));
#endif
        public static Task<HashSet<T>> ToHashSet<T>(this Task<List<T>> source) => source.Continue(s => s?.ToHashSet() ?? new HashSet<T>());
        public static Task<HashSet<T>> ToHashSet<T>(this Task<IEnumerable<T>> source) => source.Continue(s => s?.ToHashSet() ?? new HashSet<T>());
        public static ValueTask<HashSet<T>> ToHashSet<T>(this ValueTask<List<T>> source) => source.Continue(s => s?.ToHashSet() ?? new HashSet<T>());
        public static ValueTask<HashSet<T>> ToHashSet<T>(this ValueTask<IEnumerable<T>> source) => source.Continue(s => s?.ToHashSet() ?? new HashSet<T>());

        public static AtomicHashSet<T> ToAtomicHashSet<T>(this IEnumerable<T> source) => new AtomicHashSet<T>(source);
        public static Task<AtomicHashSet<T>> ToAtomicHashSet<T>(this Task<List<T>> source) => source.Continue(s => s.NotNull(nameof(source)).ToAtomicHashSet());
        public static Task<AtomicHashSet<T>> ToAtomicHashSet<T>(this Task<IEnumerable<T>> source) => source.Continue(s => s.NotNull(nameof(source)).ToAtomicHashSet());

        public static AsyncQueue<T> ToAsyncQueue<T>(this IEnumerable<T> source) => new AsyncQueue<T>(source);
        public static AtomicQueue<T> ToAtomicQueue<T>(this IEnumerable<T> source) => new AtomicQueue<T>(source);

        public static Stack<T> ToStack<T>(this IEnumerable<T> items) => new Stack<T>(items);
        public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> source)
        {
            source.NotNull(nameof(source));
            var items = new Dictionary<K, V>();
            foreach (var item in source)
                items.Add(item.Key, item.Value);
            return items;
        }
    }
}