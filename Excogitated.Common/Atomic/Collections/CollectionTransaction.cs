using Excogitated.Common.Extensions;
using System;

namespace Excogitated.Common.Atomic.Collections
{
    public class CollectionTransaction<T> : IDisposable
    {
        private readonly AtomicBool _completed = new();
        private readonly IAtomicCollection<CollectionTransaction<T>> _items;

        public T Item { get; }

        public void Complete() => _completed.Value = true;

        public CollectionTransaction(IAtomicCollection<CollectionTransaction<T>> items, T item)
        {
            _items = items.NotNull(nameof(items));
            Item = item;
        }

        public void Dispose()
        {
            if (!_completed)
                _items.Add(this);
        }
    }

    public static class ExtCollectionTransaction
    {
        public static void Add<T>(this IAtomicCollection<CollectionTransaction<T>> items, T item) => items.Add(new CollectionTransaction<T>(items, item));
    }
}