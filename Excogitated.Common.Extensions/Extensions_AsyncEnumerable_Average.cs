using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Excogitated.Extensions
{
    public static class Extensions_AsyncEnumerable_Average
    {
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

        public static ValueTask<double> AverageOrZero(this IAsyncEnumerable<int> values) => values.AverageOrZero(i => i);
        public static ValueTask<double> AverageOrZero(this IAsyncEnumerable<long> values) => values.AverageOrZero(i => i);
        public static ValueTask<double> AverageOrZero(this IAsyncEnumerable<decimal> values) => values.AverageOrZero(i => i.ToDouble());
        public static ValueTask<double> AverageOrZero<T>(this IAsyncEnumerable<T> values, Func<T, double> selector) => values.Select(selector).AverageOrZero();
        public static async ValueTask<double> AverageOrZero(this IAsyncEnumerable<double> values)
        {
            values.NotNull(nameof(values));
            var sum = 0d;
            var count = 0L;
            await foreach (var value in values)
            {
                sum += value;
                count++;
            }
            return count == 0 ? 0 : sum / count;
        }
    }
}