using System;

namespace Excogitated.Common.Extensions
{
    public static class Extensions_Numbers
    {
        public static string ToMoney(this float value) => value.ToString("c");
        public static string ToMoney(this float current, float initial) => $"{current.ToMoney()} {(current / initial).ToPercent()}";
        public static string ToPercent(this float value) => $"{(value >= 0 ? "+" : string.Empty)}{Math.Round(value * 100, 2)}%";

        public static string ToMoney(this double value) => value.ToString("c");
        public static string ToMoney(this double current, double initial) => $"{current.ToMoney()} {(current / initial).ToPercent()}";
        public static string ToPercent(this double value) => $"{(value >= 0 ? "+" : string.Empty)}{Math.Round(value * 100, 2)}%";

        public static string ToMoney(this decimal value) => value.ToString("c");
        public static string ToMoney(this decimal current, decimal initial) => $"{current.ToMoney()} {(current / initial).ToPercent()}";
        public static string ToPercent(this decimal value) => $"{(value >= 0 ? "+" : string.Empty)}{Math.Round(value * 100, 2)}%";

        public static double ToDouble(this int value) => value;
        public static double ToDouble(this long value) => value;
        public static double ToDouble(this decimal value) => decimal.ToDouble(value);

        public static decimal ToDecimal(this int value) => value;
        public static decimal ToDecimal(this long value) => value;
        public static decimal ToDecimal(this double value) => new decimal(value);

        public static int ToInt(this long value) => value > int.MaxValue ? int.MaxValue : value < int.MinValue ? int.MinValue : (int)value;

        public static decimal CalculateChangePercentage(this decimal first, decimal second)
        {
            if (second == 0)
                second = first;
            if (second == 0)
                return 0;
            return first / second - 1;
        }

        public static string Format(this TimeSpan time)
        {
            if (time < TimeSpan.Zero)
                return $"-{TimeSpan.FromTicks(time.Ticks * -1).Format()}";
            if (time.TotalDays >= 1)
                return time.ToString(@"d\.hh\:mm\:ss");
            return time.ToString(@"hh\:mm\:ss");
        }
    }
}
