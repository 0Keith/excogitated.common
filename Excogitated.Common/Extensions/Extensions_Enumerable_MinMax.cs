using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common
{
    public static class Extensions_Enumerable_MinMax
    {
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

        public static T MinSelect<T, R>(this IEnumerable<T> values, Func<T, R> minFunc) where R : IComparable<R>
        {
            values.NotNull(nameof(values));
            minFunc.NotNull(nameof(minFunc));
            using var v = values.GetEnumerator();
            var min = v.MoveNext() ? v.Current : default;
            while (v.MoveNext())
                if (minFunc(min).CompareTo(minFunc(v.Current)) > 0)
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

        public static T MaxSelect<T, R>(this IEnumerable<T> values, Func<T, R> maxFunc) where R : IComparable<R>
        {
            values.NotNull(nameof(values));
            maxFunc.NotNull(nameof(maxFunc));
            using var v = values.GetEnumerator();
            var max = v.MoveNext() ? v.Current : default;
            while (v.MoveNext())
                if (maxFunc(max).CompareTo(maxFunc(v.Current)) < 0)
                    max = v.Current;
            return max;
        }
    }
}