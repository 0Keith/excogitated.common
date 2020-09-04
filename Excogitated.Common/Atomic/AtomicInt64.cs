using System.Threading;

namespace Excogitated.Common.Atomic
{
    public class AtomicInt64 : ICounter<long>
    {
        public static implicit operator long(AtomicInt64 counter) => counter.Value;

        private long _value;

        public AtomicInt64(long initial = 0)
        {
            _value = initial;
        }

        public long Value
        {
            get => _value;
            set => Interlocked.Exchange(ref _value, value);
        }

        public override string ToString() => Value.ToString();
        public override bool Equals(object obj) => obj is ICounter<long> c && c.Value == Value;
        public override int GetHashCode() => Value.GetHashCode();

        public long Increment() => Interlocked.Increment(ref _value);
        public long Decrement() => Interlocked.Decrement(ref _value);

        public long Add(long value) => Interlocked.Add(ref _value, value);

        public bool TrySet(long value, long expected) => Interlocked.CompareExchange(ref _value, value, expected) == expected;

        public bool TryAdd(long value, long maxValue)
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

        public long GetAndSet(long value) => Interlocked.Exchange(ref _value, value);
    }
}