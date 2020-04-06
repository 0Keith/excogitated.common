using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public static class Extensions_AsyncEnumerable_To
    {
        public static async ValueTask<List<T>> ToList<T>(this IAsyncEnumerable<T> source)
        {
            source.NotNull(nameof(source));
            var items = new List<T>();
            await foreach (var item in source)
                items.Add(item);
            return items;
        }

        public static async ValueTask<HashSet<T>> ToHashSet<T>(this IAsyncEnumerable<T> source)
        {
            source.NotNull(nameof(source));
            var items = new HashSet<T>();
            await foreach (var item in source)
                items.Add(item);
            return items;
        }

        public static async ValueTask<IDictionary<K, V>> ToDictionary<K, V>(this IAsyncEnumerable<KeyValuePair<K, V>> source)
        {
            source.NotNull(nameof(source));
            var items = new Dictionary<K, V>().ToInterface();
            await foreach (var i in source)
                items.Add(i);
            return items;
        }

        public static async ValueTask<IDictionary<K, V>> ToDictionary<K, V>(this IAsyncEnumerable<V> source, Func<V, K> keySelector)
        {
            source.NotNull(nameof(source));
            keySelector.NotNull(nameof(keySelector));
            var items = new Dictionary<K, V>().ToInterface();
            await foreach (var item in source)
            {
                var key = keySelector(item);
                items.Add(key, item);
            }
            return items;
        }

        public static async ValueTask<IDictionary<K, V>> ToDictionary<T, K, V>(this IAsyncEnumerable<T> source, Func<T, K> keySelector, Func<T, V> valueSelector)
        {
            source.NotNull(nameof(source));
            keySelector.NotNull(nameof(keySelector));
            valueSelector.NotNull(nameof(valueSelector));
            var items = new Dictionary<K, V>().ToInterface();
            await foreach (var item in source)
            {
                var key = keySelector(item);
                var value = valueSelector(item);
                items.Add(key, value);
            }
            return items;
        }
    }
}