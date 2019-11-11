using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Excogitated.Common
{
    public class AtomicStack<T> : IAtomicCollection<T>
    {
        private readonly Stack<T> _items = new Stack<T>();

        public int Count => _items.Count;

        public void Add(T item)
        {
            lock (this)
                _items.Push(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            lock (this)
                foreach (var item in items)
                    _items.Push(item);
        }

        public bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public bool TryRemove(T item)
        {
            lock (this)
                if (_items.Contains(item))
                {
                    AddRange(GetAndClear().Except(new[] { item }));
                    return true;
                }
            return false;
        }

        public bool TryConsume(out T item)
        {
            lock (this)
            {
                if (_items.Count > 0)
                {
                    item = _items.Pop();
                    return true;
                }
                item = default;
                return false;
            }
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

        public void Clear()
        {
            lock (this)
                _items.Clear();
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