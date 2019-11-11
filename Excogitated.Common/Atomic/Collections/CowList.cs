using System.Collections;
using System.Collections.Generic;

namespace Excogitated.Common
{
    public class CowList<T> : IAtomicCollection<T>
    {
        private List<T> _items = new List<T>();
        private List<T> CopyItems() => new List<T>(_items);

        public int Count => _items.Count;

        public void Add(T item)
        {
            lock (this)
            {
                var copy = CopyItems();
                copy.Add(item);
                _items = copy;
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            lock (this)
            {
                var copy = CopyItems();
                copy.AddRange(items);
                _items = copy;
            }
        }

        public bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public bool TryRemove(T item)
        {
            lock (this)
            {
                var copy = CopyItems();
                var removed = copy.Remove(item);
                _items = copy;
                return removed;
            }
        }

        public bool TryConsume(out T item)
        {
            lock (this)
            {
                if (_items.Count == 0)
                {
                    item = default;
                    return false;
                }

                var copy = CopyItems();
                var i = copy.Count - 1;
                item = copy[i];
                copy.RemoveAt(i);
                _items = copy;
                return true;
            }
        }

        public IEnumerable<T> GetAndClear()
        {
            lock (this)
            {
                var copy = CopyItems();
                _items.Clear();
                return copy;
            }
        }

        public void Clear()
        {
            lock (this)
                _items = new List<T>();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    }
}