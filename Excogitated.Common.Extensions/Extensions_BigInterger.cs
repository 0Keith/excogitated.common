using System;
using System.Collections.Generic;
using System.Numerics;

namespace Excogitated.Extensions
{
    public static class Extensions_BigInteger
    {
        public static BigInteger ToBigInt(this int value) => value;
        public static BigInteger ToBigInt(this long value) => value;

        public static string ToBase2(this BigInteger value) => value.ToBase("01");
        public static string ToBase10(this BigInteger value) => value.ToBase("0123456789");
        public static string ToBase16(this BigInteger value) => value.ToBase("0123456789ABCDEF");
        public static string ToBase26(this BigInteger value) => value.ToBase("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        public static string ToBase52(this BigInteger value) => value.ToBase("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
        public static string ToBase64(this BigInteger value) => value.ToBase("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/");

        public static string ToBase(this BigInteger value, string digits)
        {
            var radix = digits.NotNullOrWhitespace(nameof(digits)).Length;
            if (radix < 2)
                throw new ArgumentException("At least 2 digits are necessary to comprise a number system");
            if (value == 0)
                return digits[0].ToString();

            var chars = new Stack<char>();
            var negative = value < 0;
            while (value != 0)
            {
                value = BigInteger.DivRem(value, radix, out var remainder);
                if (remainder < 0)
                    remainder *= -1;
                chars.Push(digits[(int)remainder]);
            }
            if (negative)
                chars.Push('-');
            return new string(chars.ToArray());
        }
    }
}
