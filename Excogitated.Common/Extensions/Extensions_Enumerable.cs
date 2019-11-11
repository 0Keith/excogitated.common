﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Excogitated.Common
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

    public static class Extensions_Enumerable
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

        public static async Task<HashSet<T>> ToHashSet<T>(this Task<List<T>> source) => (await source).ToHashSet();
        public static async Task<HashSet<T>> ToHashSet<T>(this Task<IEnumerable<T>> source) => (await source).ToHashSet();

        public static AtomicHashSet<T> ToAtomicHashSet<T>(this IEnumerable<T> source) => new AtomicHashSet<T>(source);
        public static async Task<AtomicHashSet<T>> ToAtomicHashSet<T>(this Task<List<T>> source) => (await source).ToAtomicHashSet();
        public static async Task<AtomicHashSet<T>> ToAtomicHashSet<T>(this Task<IEnumerable<T>> source) => (await source).ToAtomicHashSet();

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> items) => items.OrderBy(i => Rng.Pseudo.GetInt32());

        public static Stack<T> ToStack<T>(this IEnumerable<T> items) => new Stack<T>(items);

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

        public static IEnumerable<T> Watch<T>(this IEnumerable<T> items, int estimatedTotal = 0, [CallerMemberName] string name = null)
        {
            using var _watch = new AtomicWatch(estimatedTotal, name);
            foreach (var i in items.NotNull(nameof(items)))
            {
                yield return i;
                _watch.Increment();
            }
        }

        public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> source) => new Dictionary<K, V>(source);

        public static IEnumerable<T> Distinct<T, K>(this IEnumerable<T> items, Func<T, K> keySelector)
        {
            items.NotNull(nameof(items));
            keySelector.NotNull(nameof(keySelector));
            var keys = new HashSet<K>();
            foreach (var i in items)
                if (keys.Add(keySelector(i)))
                    yield return i;
        }

        public static R MinOrDefault<T, R>(this IEnumerable<T> values, Func<T, R> selector)
            where R : IComparable<R> => values.Select(selector).MinOrDefault();
        public static T MinOrDefault<T>(this IEnumerable<T> values) where T : IComparable<T>
        {
            values.NotNull(nameof(values));
            using var v = values.GetEnumerator();
            var min = v.MoveNext() ? v.Current : default;
            while (v.MoveNext())
                if (min.CompareTo(v.Current) > 0)
                    min = v.Current;
            return min;
        }

        public static R MaxOrDefault<T, R>(this IEnumerable<T> values, Func<T, R> selector)
            where R : IComparable<R> => values.Select(selector).MaxOrDefault();
        public static T MaxOrDefault<T>(this IEnumerable<T> values) where T : IComparable<T>
        {
            values.NotNull(nameof(values));
            using var v = values.GetEnumerator();
            var max = v.MoveNext() ? v.Current : default;
            while (v.MoveNext())
                if (max.CompareTo(v.Current) < 0)
                    max = v.Current;
            return max;
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
    }
}
