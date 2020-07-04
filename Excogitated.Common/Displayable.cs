using System;

namespace Excogitated.Common
{
    public static class Displayable
    {
        public static Displayable<T> Create<T>(T value, string valueString) where T : IEquatable<T>, IComparable<T> => new Displayable<T>(value, valueString);
        public static Displayable<float> ToPercentage(this float value, int decimals = 2) => Create(value, $"{Math.Round(value * 100, decimals)}%");
        public static Displayable<double> ToPercentage(this double value, int decimals = 2) => Create(value, $"{Math.Round(value * 100, decimals)}%");
        public static Displayable<decimal> ToPercentage(this decimal value, int decimals = 2) => Create(value, $"{Math.Round(value * 100, decimals)}%");
    }

    /// <summary>
    /// Value type that stores a value and the text to be used for display purposes.
    /// Uses the underlying value for comparison rather than the text value.
    /// </summary>
    public struct Displayable<T> : IEquatable<Displayable<T>>, IComparable<Displayable<T>> where T : IEquatable<T>, IComparable<T>
    {
        public T Value { get; }
        public string Text { get; }

        public Displayable(T value, string text)
        {
            Value = value.NotNull(nameof(value));
            Text = text ?? string.Empty;
        }

        public bool Equals(Displayable<T> other) => Value.Equals(other.Value);
        public int CompareTo(Displayable<T> other) => Value.CompareTo(other.Value);
        public override bool Equals(object obj) => obj is Displayable<T> o && Equals(o);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Text;

        public static bool operator ==(Displayable<T> left, Displayable<T> right) => left.Equals(right);
        public static bool operator !=(Displayable<T> left, Displayable<T> right) => !left.Equals(right);
    }
}
