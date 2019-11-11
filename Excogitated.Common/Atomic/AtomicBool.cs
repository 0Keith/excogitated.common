namespace Excogitated.Common
{
    public class AtomicBool : ICounter<bool>
    {
        public static implicit operator bool(AtomicBool counter) => counter.Value;

        private readonly AtomicInt32 _counter = new AtomicInt32();

        public bool Value
        {
            get => _counter.Value == 1;
            set => _counter.Value = value ? 1 : 0;
        }

        public AtomicBool(bool initial = false)
        {
            Value = initial;
        }

        public override string ToString() => Value.ToString();
        public override bool Equals(object obj) => obj is ICounter<bool> c && c.Value == Value;
        public override int GetHashCode() => Value.GetHashCode();

        public bool Increment() => Value = !Value;
        public bool Decrement() => Value = !Value;

        public bool Add(bool value) => Value = Value && value;

        public bool TrySet(bool value) => TrySet(value, !value);

        public bool TrySet(bool value, bool expected) => _counter.TrySet(value ? 1 : 0, expected ? 1 : 0);

        public bool TryAdd(bool value, bool maxValue) => TrySet(value, maxValue);

        public bool GetAndSet(bool value) => _counter.GetAndSet(value ? 1 : 0) == 1;
    }
}