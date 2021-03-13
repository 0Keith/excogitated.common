using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Extensions
{
    public static class Extensions_AsyncEnumerable_GroupBy
    {
        public static IAsyncEnumerable<IGrouping<R, T>> GroupBy<T, R>(this IAsyncEnumerable<T> source, Func<T, R> keySelector) where R : IComparable<R>
        {
            keySelector.NotNull(nameof(keySelector));
            return source.GroupBy(i => new ValueTask<R>(keySelector(i)));
        }

        public static async IAsyncEnumerable<IGrouping<R, T>> GroupBy<T, R>(this IAsyncEnumerable<T> source, Func<T, ValueTask<R>> keySelector) where R : IComparable<R>
        {
            source.NotNull(nameof(source));
            keySelector.NotNull(nameof(keySelector));
            var groupings = new List<SimpleGrouping<R, T>>();
            var groupingsByKey = new Dictionary<R, SimpleGrouping<R, T>>();
            await foreach (var item in source)
            {
                var key = await keySelector(item);
                if (!groupingsByKey.TryGetValue(key, out var grouping))
                {
                    grouping = new SimpleGrouping<R, T>(key);
                    groupingsByKey[key] = grouping;
                    groupings.Add(grouping);
                }
                grouping.AddItem(item);
            }
            foreach (var g in groupings)
                yield return g;
        }
    }

    public class SimpleGrouping<R, T> : IGrouping<R, T> where R : IComparable<R>
    {
        private readonly List<T> _items = new();

        public void AddItem(T item) => _items.Add(item);
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        public R Key { get; }
        public SimpleGrouping(R key)
        {
            Key = key;
        }
    }
}