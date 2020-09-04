namespace Excogitated.Common
{
    public class Average
    {
        private double _sum;
        private double _count;

        public double Value => _count == 0 ? 0 : _sum / _count;

        public void Clear() => _sum = _count = 0;

        public void Add(double value)
        {
            _sum += value;
            _count++;
        }

        public override bool Equals(object obj) => obj is Average a && a.Value == Value;
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }
}