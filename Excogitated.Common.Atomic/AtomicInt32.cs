using System.Threading;

namespace Excogitated.Common.Atomic
{
    public class AtomicInt32 : ICounter<int>
    {
        public static implicit operator int(AtomicInt32 counter) => counter.Value;

        private int _value;

        public int Value
        {
            get => _value;
            set => Interlocked.Exchange(ref _value, value);
        }

        public AtomicInt32(int value = 0)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
        public override bool Equals(object obj) => obj is ICounter<int> c && c.Value == Value;
        public override int GetHashCode() => Value.GetHashCode();

        public int Increment() => Interlocked.Increment(ref _value);
        public int Decrement() => Interlocked.Decrement(ref _value);

        public int Add(int value) => Interlocked.Add(ref _value, value);

        public bool TrySet(int value, int expected) => Interlocked.CompareExchange(ref _value, value, expected) == expected;

        public bool TryAdd(int value, int maxValue)
        {
            if (value < maxValue)
            {
                var expected = _value;
                var desired = expected + value;
                while (desired < maxValue)
                {
                    if (TrySet(desired, expected))
                        return true;
                    expected = _value;
                    desired = expected + value;
                }
                return false;
            }
            if (value > maxValue)
            {
                var expected = _value;
                var desired = expected + value;
                while (desired > maxValue)
                {
                    if (TrySet(desired, expected))
                        return true;
                    expected = _value;
                    desired = expected + value;
                }
                return false;
            }
            return false;
        }

        public int GetAndSet(int value) => Interlocked.Exchange(ref _value, value);
    }
}