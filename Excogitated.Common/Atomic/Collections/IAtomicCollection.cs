using System.Collections.Generic;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public interface IAtomicCollection<T> : IEnumerable<T>
    {
        int Count { get; }
        void Add(T item);
        void AddRange(IEnumerable<T> items);
        bool TryAdd(T item);
        bool TryRemove(T item);
        bool TryConsume(out T item);
        ValueTask<Result<T>> TryConsumeAsync(int millisecondsTimeout = 1000);
        IEnumerable<T> GetAndClear();
        void Clear();
    }

    public static class Extensions_IAtomicCollection
    {
        public static TItem AddAndGet<T, TItem>(this IAtomicCollection<T> items, TItem item) where TItem : T
        {
            items.NotNull(nameof(items)).Add(item);
            return item;
        }

        public static IEnumerable<T> Consume<T>(this IAtomicCollection<T> items)
        {
            items.NotNull(nameof(items));
            while (items.TryConsume(out var item))
                yield return item;
        }

        public static async IAsyncEnumerable<T> ConsumeAsync<T>(this IAtomicCollection<T> items, int millisecondsTimeout = 1000)
        {
            items.NotNull(nameof(items));
            if (millisecondsTimeout <= 0)
                millisecondsTimeout = 1000;
            var result = await items.TryConsumeAsync(millisecondsTimeout);
            while (result.HasValue)
            {
                yield return result.Value;
                result = await items.TryConsumeAsync(millisecondsTimeout);
            }
        }
    }
}