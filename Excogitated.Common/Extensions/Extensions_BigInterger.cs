using Excogitated.Common.Atomic.Collections;
using Excogitated.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Excogitated.Common.Extensions
{
    public static class Extensions_BigInteger
    {
        private static readonly CowDictionary<string, Dictionary<char, int>> _digitsCache = new CowDictionary<string, Dictionary<char, int>>();
        public static BigInteger ToBase10(this string value, string digits)
        {
            if (value.IsNullOrWhiteSpace())
                return 0;
            var radix = digits.NotNullOrWhitespace(nameof(digits)).Length;
            if (radix < 2)
                throw new ArgumentException("At least 2 digits are necessary to comprise a number system");

            var cache = _digitsCache.GetOrAdd(digits, k => k.Select((c, i) => KeyValuePair.Create(c, i)).ToDictionary());
            var negative = value[0] == '-';
            BigInteger result = 0;
            for (var i = negative ? 1 : 0; i < value.Length; i++)
            {
                if (!cache.TryGetValue(value[i], out var index))
                    throw new ArgumentException($"Invalid character: {value[i]}");
                result = result * radix + index;
            }
            if (negative)
                result *= -1;
            return result;
        }
    }
}
