using System.Globalization;

namespace Excogitated.Models
{
    public struct Currency
    {
        public static implicit operator Currency(string value) => TryParse(value, out var m) ? m : default;
        public static implicit operator string(Currency money) => money.ToString();

        public static implicit operator Currency(decimal value) => new(value);
        public static implicit operator decimal(Currency money) => money.Value;

        public static bool TryParse(string value, out Currency money)
        {
            var success = decimal.TryParse(value, NumberStyles.Currency, null, out var parsed);
            money = parsed;
            return success;
        }

        public decimal Value { get; }

        public Currency(decimal value)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString("c");
        public override bool Equals(object obj) => obj is Currency m && m.Value == Value;
        public override int GetHashCode() => Value.GetHashCode();
    }
}