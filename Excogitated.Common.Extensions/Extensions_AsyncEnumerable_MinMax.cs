using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Excogitated.Common.Extensions
{
    public static class Extensions_AsyncEnumerable_MinMax
    {
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

        public static async ValueTask<T> MinSelect<T, R>(this IAsyncEnumerable<T> values, Func<T, R> minFunc) where R : IComparable<R>
        {
            values.NotNull(nameof(values));
            minFunc.NotNull(nameof(minFunc));
            await using var v = values.GetAsyncEnumerator();
            var min = await v.MoveNextAsync() ? v.Current : default;
            while (await v.MoveNextAsync())
                if (minFunc(min).CompareTo(minFunc(v.Current)) > 0)
                    min = v.Current;
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

        public static async ValueTask<T> MaxSelect<T, R>(this IAsyncEnumerable<T> values, Func<T, R> maxFunc) where R : IComparable<R>
        {
            values.NotNull(nameof(values));
            maxFunc.NotNull(nameof(maxFunc));
            await using var v = values.GetAsyncEnumerator();
            var max = await v.MoveNextAsync() ? v.Current : default;
            while (await v.MoveNextAsync())
                if (maxFunc(max).CompareTo(maxFunc(v.Current)) < 0)
                    max = v.Current;
            return max;
        }
    }
}