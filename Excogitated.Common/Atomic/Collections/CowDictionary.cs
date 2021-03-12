using Excogitated.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Atomic.Collections
{
    public class CowDictionary<TKey, TValue> : IAtomicDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _items = new();
        private Dictionary<TKey, TValue> CopyItems() => new(_items);

        public int Count => _items.Count;

        public IEnumerable<TKey> Keys => this.Select(p => p.Key);
        public IEnumerable<TValue> Values => this.Select(p => p.Value);

        public TValue this[TKey key]
        {
            get => GetOrDefault(key);
            set => AddOrSet(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public void Add(TKey key, TValue value)
        {
            lock (this)
            {
                var copy = CopyItems();
                copy.Add(key, value);
                _items = copy;
            }
        }

        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            lock (this)
            {
                var copy = CopyItems();
                foreach (var item in items)
                    copy.Add(item.Key, item.Value);
                _items = copy;
            }
        }

        public void AddOrSet(TKey key, TValue value)
        {
            lock (this)
            {
                var copy = CopyItems();
                copy[key] = value;
                _items = copy;
            }
        }

        public bool TryAdd(KeyValuePair<TKey, TValue> item) => TryAdd(item.Key, item.Value);
        public bool TryAdd(TKey key, TValue value)
        {
            lock (this)
            {
                if (_items.ContainsKey(key))
                    return false;
                var copy = CopyItems();
                copy.Add(key, value);
                _items = copy;
                return true;
            }
        }

        public bool TryConsume(out KeyValuePair<TKey, TValue> item)
        {
            lock (this)
            {
                if (_items.Count == 0)
                {
                    item = default;
                    return false;
                }

                var copy = CopyItems();
                item = copy.FirstOrDefault();
                copy.Remove(item.Key);
                _items = copy;
                return true;
            }
        }

        public bool TryRemove(KeyValuePair<TKey, TValue> item) => TryRemove(item.Key);
        public bool TryRemove(TKey key)
        {
            lock (this)
            {
                if (_items.Count == 0)
                    return false;
                var copy = CopyItems();
                if (copy.Remove(key))
                {
                    _items = copy;
                    return true;
                }
                return false;
            }
        }

        public bool ContainsKey(TKey key) => _items.ContainsKey(key);

        public bool TryGetValue(TKey key, out TValue value) => _items.TryGetValue(key, out value);
        public TValue GetOrDefault(TKey key) => TryGetValue(key, out var value) ? value : default;

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
        {
            if (TryGetValue(key, out var value))
                return value;
            value = factory.NotNull(nameof(factory)).Invoke(key);
            lock (this)
            {
                if (TryGetValue(key, out var current))
                    return current;
                var copy = CopyItems();
                copy.Add(key, value);
                _items = copy;
                return value;
            }
        }

        public void Clear()
        {
            lock (this)
                _items = new Dictionary<TKey, TValue>();
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> GetAndClear()
        {
            lock (this)
            {
                var items = _items;
                _items = new Dictionary<TKey, TValue>();
                return items;
            }
        }

        public void ClearAndAdd(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            lock (this)
            {
                var copy = new Dictionary<TKey, TValue>();
                if (items is object)
                    foreach (var item in items)
                        copy.Add(item.Key, item.Value);
                _items = copy;
            }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> GetAndClearAndAdd(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            lock (this)
            {
                var current = _items;
                ClearAndAdd(items);
                return current;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _items.GetEnumerator();
    }
}