using Excogitated.Common.Atomic.Collections;
using Excogitated.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Excogitated.Common
{
    public static class Rng
    {
        private static readonly ThreadLocal<Random> _rng = ThreadLocal.Create<Random>();

        public static bool GetBit() => _rng.Value.Next() % 2 == 0;
        public static byte GetByte() => (byte)_rng.Value.Next(0, 256);
        public static bool[] GetBits(int count) => GetBytes(count).Select(b => b % 2 == 0).ToArray();
        public static byte[] GetBytes(int count)
        {
            var bytes = new byte[count];
            _rng.Value.NextBytes(bytes);
            return bytes;
        }

        public static int GetInt32() => _rng.Value.Next();
        public static int GetInt32(int maxInclusive) => _rng.Value.Next(maxInclusive + 1);
        public static int GetInt32(int minInclusive, int maxInclusive) => _rng.Value.Next(minInclusive, maxInclusive + 1);

        public static long GetInt64() => (long)(long.MaxValue * GetDouble());
        public static long GetInt64(long maxInclusive) => GetInt64(0, maxInclusive);
        public static long GetInt64(long minInclusive, long maxInclusive)
        {
            var range = Math.Abs(maxInclusive - minInclusive) + 1;
            var d = range * _rng.Value.NextDouble();
            return (long)(minInclusive + d);
        }

        public static double GetDouble() => _rng.Value.NextDouble();
        public static double GetDouble(double maxInclusive) => GetDouble(0, maxInclusive);
        public static double GetDouble(double minInclusive, double maxInclusive)
        {
            var range = Math.Abs(maxInclusive - minInclusive) + 1;
            var d = range * _rng.Value.NextDouble();
            return minInclusive + d;
        }

        public static decimal GetDecimal() => new decimal(GetInt32(), GetInt32(), GetInt32(), GetBit(), 2);

        public static T SelectOne<T>(ReadOnlySpan<T> possibilities)
        {
            var selection = GetInt32(0, possibilities.Length - 1);
            return possibilities[selection];
        }

        public static T SelectOne<T>(T[] possibilities)
        {
            var selection = GetInt32(0, possibilities.Length - 1);
            return possibilities[selection];
        }

        public static T SelectOne<T>(IList<T> possibilities)
        {
            var selection = GetInt32(0, possibilities.Count - 1);
            return possibilities[selection];
        }

        public static char SelectOne(string possibilities)
        {
            var selection = GetInt32(0, possibilities.Length - 1);
            return possibilities[selection];
        }

        private static readonly CowDictionary<Type, Array> _enumValues = new CowDictionary<Type, Array>();
        public static T SelectOne<T>() where T : Enum
        {
            var values = _enumValues.GetOrAdd(typeof(T), k => Enum.GetValues(k));
            var selection = GetInt32(0, values.Length - 1);
            return (T)values.GetValue(selection);
        }

        public static string GetText(int length, string includedCharacters)
        {
            var b = new StringBuilder(length);
            for (var i = 0; i < length; i++)
                b.Append(SelectOne(includedCharacters));
            return b.ToString();
        }
    }
}