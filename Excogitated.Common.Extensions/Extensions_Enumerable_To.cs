using Excogitated.Common.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common.Extensions
{
    public static class Extensions_Enumerable_To
    {
#if NETSTANDARD2_0
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) => new(source);
#endif
        public static Task<HashSet<T>> ToHashSet<T>(this Task<List<T>> source) => source.Continue(s => s?.ToHashSet() ?? new HashSet<T>());
        public static Task<HashSet<T>> ToHashSet<T>(this Task<IEnumerable<T>> source) => source.Continue(s => s?.ToHashSet() ?? new HashSet<T>());
        public static ValueTask<HashSet<T>> ToHashSet<T>(this ValueTask<List<T>> source) => source.Continue(s => s?.ToHashSet() ?? new HashSet<T>());
        public static ValueTask<HashSet<T>> ToHashSet<T>(this ValueTask<IEnumerable<T>> source) => source.Continue(s => s?.ToHashSet() ?? new HashSet<T>());

        public static Stack<T> ToStack<T>(this IEnumerable<T> items) => new(items);
#if NET5_0
        public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> source)
        {
            source.NotNull(nameof(source));
            var items = new Dictionary<K, V>(source);
            return items;
        }
#endif
    }
}