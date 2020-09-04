using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Excogitated.Common.Atomic.Collections
{
    public class AtomicHashSet<T> : IAtomicCollection<T>
    {
        private readonly HashSet<T> _items;

        public AtomicHashSet(IEnumerable<T> source = null)
        {
            if (source != null)
                _items = new HashSet<T>(source);
            else
                _items = new HashSet<T>();
        }

        public int Count => _items.Count;

        public void Add(T item) => TryAdd(item);

        public void AddRange(IEnumerable<T> items)
        {
            lock (this)
                foreach (var item in items)
                    _items.Add(item);
        }

        public bool TryAdd(T item)
        {
            lock (this)
                return _items.Add(item);
        }

        public bool TryRemove(T item)
        {
            lock (this)
                return _items.Remove(item);
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
                item = _items.FirstOrDefault();
                return _items.Remove(item);
            }
        }

        public void Clear()
        {
            lock (this)
                _items.Clear();
        }

        public IEnumerable<T> GetAndClear()
        {
            lock (this)
            {
                var items = _items.ToList();
                _items.Clear();
                return items;
            }
        }

        public void ClearAndAdd(IEnumerable<T> items)
        {
            lock (this)
            {
                _items.Clear();
                AddRange(items);
            }
        }

        public IEnumerable<T> GetAndClearAndAdd(IEnumerable<T> items)
        {
            lock (this)
            {
                var current = GetAndClear();
                AddRange(items);
                return current;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator()
        {
            if (Monitor.IsEntered(this))
                return _items.GetEnumerator();
            lock (this)
                return _items.ToList().GetEnumerator();
        }
    }
}