using System.Globalization;

namespace Excogitated.Common
{
    public struct Currency
    {
        public static implicit operator Currency(string value) => Parse(value);
        public static implicit operator string(Currency money) => money.ToString();

        public static implicit operator Currency(decimal value) => new Currency(value);
        public static implicit operator decimal(Currency money) => money.Value;

        public static Currency Parse(string value)
        {
            if (value is null)
                return 0;
            return decimal.Parse(value, NumberStyles.Currency);
        }

        public decimal Value { get; }

        public Currency(decimal value)
        {
            Value = value;
        }

        public override string ToString() => Value.ToMoney();
        public override bool Equals(object obj) => obj is Currency m && m.Value == Value;
        public override int GetHashCode() => Value.GetHashCode();
    }
}