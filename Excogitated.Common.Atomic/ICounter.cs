namespace Excogitated.Threading
{
    public interface ICounter<T>
    {
        T Value { get; set; }
        T Increment();
        T Decrement();
        T Add(T value);
        bool TrySet(T value, T expected);
        bool TryAdd(T value, T maxValue);
        T GetAndSet(T value);
    }
}