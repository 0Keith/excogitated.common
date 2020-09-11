using Excogitated.Common.Atomic;
using Excogitated.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Excogitated.Common.Extensions
{
    public static class Extensions_AsyncEnumerable
    {
        public static async IAsyncEnumerable<T> ToAsync<T>(this IEnumerable<T> source)
        {
            source.NotNull(nameof(source));
            foreach (var item in source)
                yield return item;
            await new ValueTask();
        }

        public static async ValueTask<T> First<T>(this IAsyncEnumerable<T> source)
        {
            source.NotNull(nameof(source));
            await foreach (var item in source)
                return item;
            throw new Exception("At least one value is required.");
        }

        public static async ValueTask<T> FirstOrDefault<T>(this IAsyncEnumerable<T> source)
        {
            source.NotNull(nameof(source));
            await foreach (var item in source)
                return item;
            return default;
        }

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

        public static async IAsyncEnumerable<T> Each<T>(this IAsyncEnumerable<T> source, Action<T> action)
        {
            source.NotNull(nameof(source));
            action.NotNull(nameof(action));
            await foreach (var item in source)
            {
                action(item);
                yield return item;
            }
        }

        public static async IAsyncEnumerable<T> Each<T>(this IAsyncEnumerable<T> source, Func<T, ValueTask> action)
        {
            source.NotNull(nameof(source));
            action.NotNull(nameof(action));
            await foreach (var item in source)
            {
                await action(item);
                yield return item;
            }
        }

        public static IAsyncEnumerable<T> WhereNotNull<T>(this IAsyncEnumerable<T> source) where T : class => source.Where(i => i is null == false);
        public static async IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> source, Func<T, bool> filter)
        {
            source.NotNull(nameof(source));
            filter.NotNull(nameof(filter));
            await foreach (var item in source)
                if (filter(item))
                    yield return item;
        }

        //public static IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> source, Func<T, Task<bool>> filter) => source.Where(filter.ToAsync());
        public static async IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> source, Func<T, ValueTask<bool>> filter)
        {
            source.NotNull(nameof(source));
            filter.NotNull(nameof(filter));
            await foreach (var item in source)
                if (await filter(item))
                    yield return item;
        }

        public static async IAsyncEnumerable<T> Distinct<T>(this IAsyncEnumerable<T> source)
        {
            source.NotNull(nameof(source));
            var values = new HashSet<T>();
            await foreach (var item in source)
                if (values.Add(item))
                    yield return item;
        }

        public static async IAsyncEnumerable<T> Distinct<T, K>(this IAsyncEnumerable<T> source, Func<T, K> keySelector)
        {
            source.NotNull(nameof(source));
            keySelector.NotNull(nameof(keySelector));
            var keys = new HashSet<K>();
            await foreach (var item in source)
                if (keys.Add(keySelector(item)))
                    yield return item;
        }

        public static async IAsyncEnumerable<T> Take<T>(this IAsyncEnumerable<T> source, long count)
        {
            source.NotNull(nameof(source));
            await foreach (var item in source)
            {
                if (count-- <= 0)
                    break;
                yield return item;
            }
        }

        public static async IAsyncEnumerable<T> Skip<T>(this IAsyncEnumerable<T> source, long count)
        {
            source.NotNull(nameof(source));
            await foreach (var item in source)
                if (count-- <= 0)
                    yield return item;
        }

        public static async IAsyncEnumerable<V> OrderByDescending<V, K>(this IAsyncEnumerable<V> source, Func<V, K> keySelector) where K : IComparable<K>
        {
            source.NotNull(nameof(source));
            var items = await source.ToList();
            foreach (var item in items.OrderByDescending(keySelector))
                yield return item;
        }

        public static async IAsyncEnumerable<V> OrderBy<V, K>(this IAsyncEnumerable<V> source, Func<V, K> keySelector) where K : IComparable<K>
        {
            source.NotNull(nameof(source));
            var items = await source.ToList();
            foreach (var item in items.OrderBy(keySelector))
                yield return item;
        }

        public static async ValueTask<TSeed> Aggregate<T, TSeed>(this IAsyncEnumerable<T> source, TSeed seed, Func<T, T, TSeed, TSeed> aggregator)
        {
            aggregator.NotNull(nameof(aggregator));
            await using var items = source.NotNull(nameof(source)).GetAsyncEnumerator();
            if (await items.MoveNextAsync())
            {
                var previous = items.Current;
                while (await items.MoveNextAsync())
                {
                    seed = aggregator(previous, items.Current, seed);
                    previous = items.Current;
                }
            }
            return seed;
        }
    }
}