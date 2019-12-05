using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Excogitated.Common
{
    public class AtomicQueue<T> : IAtomicCollection<T>
    {
        private readonly Queue<T> _items;

        public AtomicQueue()
        {
            _items = new Queue<T>();
        }

        public AtomicQueue(IEnumerable<T> source)
        {
            _items = new Queue<T>(source.NotNull(nameof(source)));
        }

        public int Count => _items.Count;

        public void Add(T item)
        {
            lock (this)
                _items.Enqueue(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            lock (this)
                foreach (var item in items)
                    _items.Enqueue(item);
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

        public bool TryPeek(out T item)
        {
            lock (this)
                if (_items.Count > 0)
                {
                    item = _items.Peek();
                    return true;
                }
            item = default;
            return false;
        }

        public bool TryConsume(out T item)
        {
            lock (this)
                if (_items.Count > 0)
                {
                    item = _items.Dequeue();
                    return true;
                }
            item = default;
            return false;
        }

        public bool TryConsumeIf(out T item, Func<T, bool> conditional)
        {
            conditional.NotNull(nameof(conditional));
            lock (this)
                if (_items.Count > 0 && conditional(_items.Peek()))
                {
                    item = _items.Dequeue();
                    return true;
                }
            item = default;
            return false;
        }

        public IEnumerable<T> ConsumeWhile(Func<T, bool> conditional)
        {
            conditional.NotNull(nameof(conditional));
            while (TryConsumeIf(out var item, conditional))
                yield return item;
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