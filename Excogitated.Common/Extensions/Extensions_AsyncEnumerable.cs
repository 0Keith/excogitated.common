using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Excogitated.Common
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

        public static ValueTask<V> Min<T, V>(this IAsyncEnumerable<T> values, Func<T, V> selector) where V : IComparable<V> => values.Select(selector).Min();
        public static async ValueTask<T> Min<T>(this IAsyncEnumerable<T> source) where T : IComparable<T>
        {
            source.NotNull(nameof(source));
            await using var values = source.GetAsyncEnumerator();
            if (await values.MoveNextAsync() == false)
                throw new Exception("At least one value is required.");
            var min = values.Current;
            while (await values.MoveNextAsync())
                if (min.CompareTo(values.Current) > 0)
                    min = values.Current;
            return min;
        }

        public static ValueTask<V> Max<T, V>(this IAsyncEnumerable<T> values, Func<T, V> selector) where V : IComparable<V> => values.Select(selector).Max();
        public static async ValueTask<T> Max<T>(this IAsyncEnumerable<T> source) where T : IComparable<T>
        {
            source.NotNull(nameof(source));
            await using var values = source.GetAsyncEnumerator();
            if (await values.MoveNextAsync() == false)
                throw new Exception("At least one value is required.");
            var max = values.Current;
            while (await values.MoveNextAsync())
                if (max.CompareTo(values.Current) < 0)
                    max = values.Current;
            return max;
        }

        public static ValueTask<R> MinOrDefault<T, R>(this IAsyncEnumerable<T> values, Func<T, R> selector)
            where R : IComparable<R> => values.Select(selector).MinOrDefault();
        public static async ValueTask<T> MinOrDefault<T>(this IAsyncEnumerable<T> source) where T : IComparable<T>
        {
            source.NotNull(nameof(source));
            await using var values = source.GetAsyncEnumerator();
            if (await values.MoveNextAsync() == false)
                return default;
            var min = values.Current;
            while (await values.MoveNextAsync())
                if (min.CompareTo(values.Current) > 0)
                    min = values.Current;
            return min;
        }

        public static ValueTask<R> MaxOrDefault<T, R>(this IAsyncEnumerable<T> values, Func<T, R> selector)
            where R : IComparable<R> => values.Select(selector).MaxOrDefault();
        public static async ValueTask<T> MaxOrDefault<T>(this IAsyncEnumerable<T> source) where T : IComparable<T>
        {
            source.NotNull(nameof(source));
            await using var values = source.GetAsyncEnumerator();
            if (await values.MoveNextAsync() == false)
                return default;
            var max = values.Current;
            while (await values.MoveNextAsync())
                if (max.CompareTo(values.Current) < 0)
                    max = values.Current;
            return max;
        }

        public static ValueTask<int> Sum<T>(this IAsyncEnumerable<T> values, Func<T, int> selector) => values.Select(selector).Sum();
        public static async ValueTask<int> Sum(this IAsyncEnumerable<int> values)
        {
            values.NotNull(nameof(values));
            var sum = 0;
            await foreach (var value in values)
                sum += value;
            return sum;
        }

        public static ValueTask<long> Sum<T>(this IAsyncEnumerable<T> values, Func<T, long> selector) => values.Select(selector).Sum();
        public static async ValueTask<long> Sum(this IAsyncEnumerable<long> values)
        {
            values.NotNull(nameof(values));
            var sum = 0L;
            await foreach (var value in values)
                sum += value;
            return sum;
        }

        public static ValueTask<double> Sum<T>(this IAsyncEnumerable<T> values, Func<T, double> selector) => values.Select(selector).Sum();
        public static async ValueTask<double> Sum(this IAsyncEnumerable<double> values)
        {
            values.NotNull(nameof(values));
            var sum = 0d;
            await foreach (var value in values)
                sum += value;
            return sum;
        }

        public static ValueTask<double> Average(this IAsyncEnumerable<int> values) => values.Average(i => i);
        public static ValueTask<double> Average(this IAsyncEnumerable<long> values) => values.Average(i => i);
        public static ValueTask<double> Average(this IAsyncEnumerable<decimal> values) => values.Average(i => i.ToDouble());
        public static ValueTask<double> Average<T>(this IAsyncEnumerable<T> values, Func<T, double> selector) => values.Select(selector).Average();
        public static async ValueTask<double> Average(this IAsyncEnumerable<double> values)
        {
            values.NotNull(nameof(values));
            var sum = 0d;
            var count = 0L;
            await foreach (var value in values)
            {
                sum += value;
                count++;
            }
            return sum / count;
        }

        public static async ValueTask<int> Count<T>(this IAsyncEnumerable<T> source)
        {
            source.NotNull(nameof(source));
            var count = 0;
            await using var items = source.GetAsyncEnumerator();
            while (await items.MoveNextAsync())
                count++;
            return count;
        }

        public static async ValueTask<long> CountLong<T>(this IAsyncEnumerable<T> source)
        {
            source.NotNull(nameof(source));
            var count = 0L;
            await using var items = source.GetAsyncEnumerator();
            while (await items.MoveNextAsync())
                count++;
            return count;
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

        public static async IAsyncEnumerable<R> Select<T, R>(this IAsyncEnumerable<T> source, Func<T, R> selector)
        {
            source.NotNull(nameof(source));
            selector.NotNull(nameof(selector));
            await foreach (var item in source)
                yield return selector(item);
        }

        //public static IAsyncEnumerable<R> Select<T, R>(this IAsyncEnumerable<T> source, Func<T, Task<R>> selector) => source.Select(selector.ToAsync());
        public static async IAsyncEnumerable<R> Select<T, R>(this IAsyncEnumerable<T> source, Func<T, ValueTask<R>> selector)
        {
            source.NotNull(nameof(source));
            selector.NotNull(nameof(selector));
            await foreach (var item in source)
                yield return await selector(item);
        }

        public static async IAsyncEnumerable<R> SelectMany<T, R>(this IAsyncEnumerable<T> source, Func<T, IEnumerable<R>> selector)
        {
            source.NotNull(nameof(source));
            selector.NotNull(nameof(selector));
            await foreach (var item in source)
                foreach (var many in selector(item))
                    yield return many;
        }

        public static async IAsyncEnumerable<R> SelectMany<T, R>(this IAsyncEnumerable<T> source, Func<T, ValueTask<IEnumerable<R>>> selector)
        {
            source.NotNull(nameof(source));
            selector.NotNull(nameof(selector));
            await foreach (var item in source)
                foreach (var many in await selector(item))
                    yield return many;
        }

        public static async IAsyncEnumerable<R> SelectMany<T, R>(this IAsyncEnumerable<T> source, Func<T, IAsyncEnumerable<R>> selector)
        {
            source.NotNull(nameof(source));
            selector.NotNull(nameof(selector));
            await foreach (var item in source)
                await foreach (var many in selector(item))
                    yield return many;
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

        public static async IAsyncEnumerable<IGrouping<R, T>> GroupBy<T, R>(this IAsyncEnumerable<T> source, Func<T, R> keySelector) where R : IComparable<R>
        {
            source.NotNull(nameof(source));
            var items = await source.ToList();
            foreach (var item in items.GroupBy(keySelector))
                yield return item;
        }

        public static ValueTask Batch<T>(this IAsyncEnumerable<T> source, Func<T, ValueTask> action) => source.Batch(0, action);
        public static async ValueTask Batch<T>(this IAsyncEnumerable<T> source, int threadCount, Func<T, ValueTask> action)
        {
            if (threadCount <= 0)
                threadCount = Environment.ProcessorCount / 2;
            action.NotNull(nameof(action));
            var @lock = new AsyncLock();
            await using var items = source.NotNull(nameof(source)).GetAsyncEnumerator();
            await Task.WhenAll(Enumerable.Range(0, threadCount).Select(i => Task.Run(async () =>
            {
                while (true)
                {
                    T item;
                    using (await @lock.EnterAsync())
                    {
                        if (!await items.MoveNextAsync())
                            return;
                        item = items.Current;
                    }
                    await action(item);
                }
            })));
        }

        public static IAsyncEnumerable<R> Batch<T, R>(this IAsyncEnumerable<T> source, Func<T, ValueTask<R>> action) => source.Batch(0, action);
        public static async IAsyncEnumerable<R> Batch<T, R>(this IAsyncEnumerable<T> source, int threadCount, Func<T, ValueTask<R>> action)
        {
            if (threadCount <= 0)
                threadCount = Environment.ProcessorCount / 2;
            action.NotNull(nameof(action));
            var @lock = new AsyncLock();
            var results = new AsyncQueue<R>();
            await using var items = source.NotNull(nameof(source)).GetAsyncEnumerator();
            var tasks = Task.WhenAll(Enumerable.Range(0, threadCount).Select(i => Task.Run(async () =>
            {
                while (true)
                {
                    T item;
                    using (await @lock.EnterAsync())
                    {
                        if (!await items.MoveNextAsync())
                            return;
                        item = items.Current;
                    }
                    results.Add(await action(item));
                }
            })));
            tasks.ContinueWith(t => results.Complete()).Catch();
            while (!tasks.IsCompleted || results.Count > 0)
            {
                var result = await results.ConsumeAsync(1000);
                if (result.HasValue)
                    yield return result.Value;
            }
            await tasks; //propogate any errors that occurred
        }
    }
}