using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public interface IRng
    {
        bool GetBit();
        byte GetByte();
        bool[] GetBits(int count);
        byte[] GetBytes(int count);
    }

    public static class Rng
    {
        public static IRng Pseudo { get; } = new PseudoRng();
        public static IRng True { get; } = Environment.ProcessorCount <= 2 ? Pseudo : new TrueRng();
    }

    internal class TrueRng : IRng
    {
        public bool GetBit() => GetBits(1)[0];
        public bool[] GetBits(int count)
        {
            var c = new AtomicInt32();
            var bits = new bool[count];
            for (var i = 0; i < count; i++)
            {
                var task = Task.Run(() => c.Increment());
                while (!task.IsCompleted)
                    c.Increment();
                bits[i] = c % 2 == 0;
            }
            return bits;
        }

        public byte GetByte() => GetBytes(1)[0];
        public byte[] GetBytes(int count)
        {
            var bits = GetBits(count * 8);
            var bytes = new byte[count];
            for (var iByte = 0; iByte < count; iByte++)
            {
                var sum = 0;
                for (var iBit = 0; iBit < 8; iBit++)
                    if (bits[iByte * 8 + iBit])
                        switch (iBit)
                        {
                            case 0: sum += 1; break;
                            case 1: sum += 2; break;
                            case 2: sum += 4; break;
                            case 3: sum += 8; break;
                            case 4: sum += 16; break;
                            case 5: sum += 32; break;
                            case 6: sum += 64; break;
                            case 7: sum += 128; break;
                        }
                bytes[iByte] = (byte)sum;
            }
            return bytes;
        }
    }

    internal class PseudoRng : IRng
    {
        private readonly ThreadLocal<Random> _rng = ThreadLocal.Create<Random>();

        public bool GetBit() => _rng.Value.Next() % 2 == 0;
        public byte GetByte() => (byte)_rng.Value.Next(0, 256);
        public bool[] GetBits(int count) => GetBytes(count).Select(b => b % 2 == 0).ToArray();
        public byte[] GetBytes(int count)
        {
            var bytes = new byte[count];
            _rng.Value.NextBytes(bytes);
            return bytes;
        }
    }

    public static class Extensions_Rng
    {
        public static int GetInt32(this IRng rng)
        {
            var bytes = rng.GetBytes(4);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static int GetInt32(this IRng rng, int maxInclusive)
        {
            var i = rng.GetInt32();
            var r = i % (maxInclusive + 1);
            if (r < 0)
                r *= -1;
            return r;
        }

        public static int GetInt32(this IRng rng, int minInclusive, int maxInclusive)
        {
            var range = Math.Abs(maxInclusive - minInclusive);
            var i = rng.GetInt32(range);
            return i + minInclusive;
        }

        public static long GetInt64(this IRng rng)
        {
            var bytes = rng.GetBytes(8);
            return BitConverter.ToInt64(bytes, 0);
        }

        public static long GetInt64(this IRng rng, long maxInclusive)
        {
            var i = rng.GetInt64();
            var r = i % (maxInclusive + 1);
            if (r < 0)
                r *= -1;
            return r;
        }

        public static long GetInt64(this IRng rng, long minInclusive, long maxInclusive)
        {
            var range = Math.Abs(maxInclusive - minInclusive);
            var i = rng.GetInt64(range);
            return i + minInclusive;
        }

        public static double GetDouble(this IRng rng) => BitConverter.ToDouble(rng.GetBytes(8), 0);

        public static decimal GetDecimal(this IRng rng) => new decimal(rng.GetInt32(), rng.GetInt32(), rng.GetInt32(), rng.GetBit(), 2);

        public static T SelectOne<T>(this IRng rng, T[] possibilities)
        {
            var selection = rng.GetInt32(0, possibilities.Length - 1);
            return possibilities[selection];
        }

        private static readonly CowDictionary<Type, Array> _enumValues = new CowDictionary<Type, Array>();
        public static T SelectOne<T>(this IRng rng) where T : Enum
        {
            var values = _enumValues.GetOrAdd(typeof(T), k => Enum.GetValues(k));
            var selection = rng.GetInt32(0, values.Length - 1);
            return (T)values.GetValue(selection);
        }
    }
}
