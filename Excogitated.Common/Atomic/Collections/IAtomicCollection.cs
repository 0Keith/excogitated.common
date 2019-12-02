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
    }
}