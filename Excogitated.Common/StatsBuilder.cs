using System;
using System.Linq;
using System.Text;

namespace Excogitated.Common
{
    public static class StatsBuilderExtensions
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

    public class StatsBuilder
    {
        private readonly AtomicList<(string Name, object Value)> _stats = new AtomicList<(string, object)>();
        private readonly string _title;

        public StatsBuilder(string title = null)
        {
            _title = title;
        }

        public StatsBuilder Add(string name, object value)
        {
            _stats.Add((name ?? string.Empty, value));
            return this;
        }

        public StatsBuilder Add(string name, double count, double total)
        {
            var value = $"{Math.Round(count, 2)} {(count / total).ToPercent()}";
            _stats.Add((name ?? string.Empty, value));
            return this;
        }

        public StatsBuilder AddSeparator()
        {
            _stats.Add((string.Empty, null));
            return this;
        }

        public override string ToString()
        {
            var message = new StringBuilder(_title).AppendLine();
            var maxLength = _stats.Max(s => s.Name.Length);
            foreach (var s in _stats)
                message.Append(s.Name.PadLeft(maxLength, ' ')).Append(" : ").Append(s.Value).AppendLine();
            return message.ToString();
        }
    }
}
