using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Excogitated.Common.Extensions
{
    public static class Extensions_AsyncEnumerable_SumCount
    {
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
    }
}