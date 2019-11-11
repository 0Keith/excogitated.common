﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Excogitated.Common
{
    public class AtomicList<T> : IAtomicCollection<T>
    {
        private readonly List<T> _items = new List<T>();

        public int Count => _items.Count;

        public void Add(T item)
        {
            lock (this)
                _items.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            lock (this)
                _items.AddRange(items);
        }

        public bool TryAdd(T item)
        {
            Add(item);
            return true;
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
                var i = _items.Count - 1;
                item = _items[i];
                _items.RemoveAt(i);
                return true;
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