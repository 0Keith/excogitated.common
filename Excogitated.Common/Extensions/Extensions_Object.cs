using System;
using System.Collections.Generic;

namespace Excogitated.Common
{
    public static class Extensions_Object
    {
        public static bool IsNotNull<T>(this T value) => value is null == false;
        public static T NotNull<T>(this T value, string name)
        {
            if (value is null)
                throw new ArgumentNullException(name);
            return value;
        }

        public static string NotNullOrWhitespace(this string value, string name) => value.IsNullOrWhiteSpace() ? throw new ArgumentNullException(name) : value;

        public static bool IsDebug(this Type type) => type?.Assembly.Location?.ToLower().Contains("debug") ?? false;
        public static bool IsRelease(this Type type) => !type.IsDebug();

        public static bool Between<T>(this IComparable<T> value, T minInclusive, T maxInclusive)
        {
            value.NotNull(nameof(value));
            var min = value.CompareTo(minInclusive);
            var max = value.CompareTo(maxInclusive);
            return min >= 0 && max <= 0;
        }

        public static bool EqualsAny<T>(this T value1, T value2)
        {
            var comparer = EqualityComparer<T>.Default;
            return comparer.Equals(value1, value2);
        }

        public static bool EqualsAny<T>(this T value1, T value2, T value3)
        {
            var comparer = EqualityComparer<T>.Default;
            return comparer.Equals(value1, value2) || comparer.Equals(value1, value3);
        }

        public static bool EqualsAny<T>(this T value1, T value2, T value3, T value4)
        {
            var comparer = EqualityComparer<T>.Default;
            return comparer.Equals(value1, value2) || comparer.Equals(value1, value3) || comparer.Equals(value1, value4);
        }

        public static bool EqualsAny<T>(this T value1, T value2, T value3, T value4, T value5)
        {
            var comparer = EqualityComparer<T>.Default;
            return comparer.Equals(value1, value2) || comparer.Equals(value1, value3) || comparer.Equals(value1, value4) || comparer.Equals(value1, value5);
        }

        public static bool EqualsAny<T>(this T value, params T[] values)
        {
            if (values is null == false)
            {
                var comparer = EqualityComparer<T>.Default;
                foreach (var v in values)
                    if (comparer.Equals(value, v))
                        return true;
            }
            return false;
        }
    }
}