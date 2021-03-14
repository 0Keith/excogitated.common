using System;
using System.Linq;
using System.Text;

namespace Excogitated.ServiceBus.Azure
{
    internal static class AzureExtensions
    {
        public static string Shorten(this string source, int maxLength, string prefix = null, char separator = '-')
        {
            var result = new StringBuilder(maxLength);
            if (prefix is string)
            {
                result.Append(prefix).Append(separator);
            }

            var remaining = maxLength - result.Length;
            if (source.Length < remaining)
            {
                result.Append(source);
            }
            else
            {
                var hash = source.GetHashCodeShort();
                result.Append(hash).Append(separator);
                remaining = maxLength - result.Length;
                var partial = source.Reverse()
                    .Take(remaining)
                    .Reverse()
                    .ToArray();
                result.Append(partial);
            }
            return result.ToString();
        }

        public static string GetHashCodeShort(this string source)
        {
            var value = source.Select(c => (uint)c)
                .Aggregate((all, next) => (all * 10) + next);
            return value.ToBase36();
        }

        public static string ToBase36(this uint value) => value.ToBase("0123456789abcdefghijklmnopqrstuvwxyz");
        public static string ToBase(this uint value, string chars)
        {
            var radix = (uint)chars.Length;
            var result = new StringBuilder();
            while (value > 0)
            {
                var index = (int)(value % radix);
                result.Append(chars[index]);
                value /= radix;
            }
            return result.ToString();
        }
    }
}
