using Excogitated.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Extensions
{
    public class DisposableEnumerable<T> : IDisposable, IEnumerable<T> where T : IDisposable
    {
        private readonly IEnumerable<T> _resources;

        public void Dispose() => _resources.Dispose();
        public IEnumerator<T> GetEnumerator() => _resources.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _resources.GetEnumerator();

        public DisposableEnumerable(IEnumerable<T> resources)
        {
            _resources = resources.NotNull(nameof(resources));
        }
    }

    public static partial class Extensions_Enumerable
    {
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> items, TKey key) => items.NotNull(nameof(items)).TryGetValue(key, out var item) ? item : default;

        public static DisposableEnumerable<T> AsDisposable<T>(this IEnumerable<T> resources) where T : IDisposable => new DisposableEnumerable<T>(resources);

        public static void Dispose<T>(this IEnumerable<T> resources) where T : IDisposable
        {
            if (resources is null) return;
            var exceptions = default(List<Exception>);
            foreach (var r in resources)
                try
                {
                    r?.Dispose();
                }
                catch (Exception e)
                {
                    if (exceptions is null)
                        exceptions = new List<Exception>();
                    exceptions.Add(e);
                }
            if (exceptions?.Count > 0)
                throw new AggregateException(exceptions);
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            action.NotNull(nameof(action));
            foreach (var i in items.NotNull(nameof(items)))
                action(i);
        }

        public static IDictionary<K, V> ToInterface<K, V>(this IDictionary<K, V> source) => source;

        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> items, bool hasCurrent)
        {
            items.NotNull(nameof(items));
            if (hasCurrent)
                yield return items.Current;
            while (items.MoveNext())
                yield return items.Current;
        }

        public static decimal AverageOrZero<T>(this IEnumerable<T> values, Func<T, decimal> selector) => values.Select(selector).AverageOrZero();
        public static decimal AverageOrZero(this IEnumerable<decimal> values)
        {
            var sum = 0m;
            var count = 0;
            foreach (var v in values.NotNull(nameof(values)))
            {
                sum += v;
                count++;
            }
            return count == 0 ? 0 : sum / count;
        }

        public static double AverageOrZero<T>(this IEnumerable<T> values, Func<T, double> selector) => values.Select(selector).AverageOrZero();
        public static double AverageOrZero(this IEnumerable<double> values)
        {
            var sum = 0d;
            var count = 0;
            foreach (var v in values.NotNull(nameof(values)))
            {
                sum += v;
                count++;
            }
            return count == 0 ? 0 : sum / count;
        }

        public static IEnumerable<R> SelectSplit<T, R>(this IEnumerable<T> values, Func<IEnumerable<T>, R> splitter)
        {
            splitter.NotNull(nameof(splitter));
            using var items = values.GetEnumerator();
            while (items.MoveNext())
                yield return splitter(items.ToEnumerable(true));
        }

        public static IEnumerable<R> SelectSplit<T, R>(this IEnumerable<T> values, Func<int, IEnumerable<T>, R> splitter)
        {
            splitter.NotNull(nameof(splitter));
            using var items = values.GetEnumerator();
            for (var i = 0; items.MoveNext(); i++)
                yield return splitter(i, items.ToEnumerable(true));
        }

        public static IEnumerable<T> Distinct<T, K>(this IEnumerable<T> items, Func<T, K> keySelector)
        {
            items.NotNull(nameof(items));
            keySelector.NotNull(nameof(keySelector));
            var keys = new HashSet<K>();
            foreach (var i in items)
                if (keys.Add(keySelector(i)))
                    yield return i;
        }

        public static IEnumerable<T> ReverseFast<T>(this IList<T> items)
        {
            items.NotNull(nameof(items));
            for (var i = items.Count - 1; i >= 0; i--)
                yield return items[i];
        }

        public static IEnumerable<T> ReverseFast<T>(this LinkedList<T> items)
        {
            items.NotNull(nameof(items));
            var node = items.Last;
            while (node.IsNotNull())
            {
                yield return node.Value;
                node = node.Previous;
            }
        }

        public static R FirstAndLastOrDefault<T, R>(this IEnumerable<T> source, Func<T, T, R> selector)
        {
            source.NotNull(nameof(source));
            selector.NotNull(nameof(selector));
            using var items = source.GetEnumeratorCounted();
            var first = items.MoveNext() ? items.Current : default;
            var last = items.MoveNext() ? items.Current : default;
            if (items.Count != 2)
                return default;
            while (items.MoveNext())
                last = items.Current;
            return selector(first, last);
        }

        public static CountedEnumerator<T> GetEnumeratorCounted<T>(this IEnumerable<T> source) => new CountedEnumerator<T>(source);

#if NETSTANDARD2_0
        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int count)
        {
            source.NotNull(nameof(source));
            var q = new Queue<T>();
            foreach (var item in source)
            {
                q.Enqueue(item);
                if (q.Count > count)
                    yield return q.Dequeue();
            }
        }
#endif

        public static TSeed Aggregate<T, TSeed>(this IEnumerable<T> source, TSeed seed, Func<T, T, TSeed, TSeed> aggregator)
        {
            aggregator.NotNull(nameof(aggregator));
            using var items = source.NotNull(nameof(source)).GetEnumerator();
            if (items.MoveNext())
            {
                var previous = items.Current;
                while (items.MoveNext())
                {
                    seed = aggregator(previous, items.Current, seed);
                    previous = items.Current;
                }
            }
            return seed;
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
        {
            if (source.IsNotNull())
                foreach (var items in source)
                    if (items.IsNotNull())
                        foreach (var item in items)
                            yield return item;
        }
    }

    public class CountedEnumerator<T> : IEnumerator<T>
    {
        private readonly IEnumerator<T> _source;

        public long Count { get; private set; }

        public T Current => _source.Current;
        object IEnumerator.Current => _source.Current;
        public void Dispose() => _source.Dispose();
        public void Reset() => _source.Reset();

        public CountedEnumerator(IEnumerable<T> source)
        {
            _source = source.NotNull(nameof(source)).GetEnumerator();
        }

        public bool MoveNext()
        {
            if (_source.MoveNext())
            {
                Count++;
                return true;
            }
            return false;
        }
    }
}
