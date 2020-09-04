using Excogitated.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Excogitated.Common.Atomic.Collections
{
    /// <summary>
    /// A synchronized Dictionary wrapper.
    /// AtomicDictionary should be used when a workload will be mostly single or lightly threaded but still needs to remain thread safe.
    /// In this scenario AtomicDictionary is faster and more efficient than ConcurrentDictionary.
    /// Enumeration will create a snapshot of the current keys and or values, use sparringly since this incurs significant overhead.
    /// </summary>
    /// <typeparam name="TKey">The Type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The Type of values in the dictionary.</typeparam>
    public class AtomicDictionary<TKey, TValue> : IAtomicDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _items = new Dictionary<TKey, TValue>();

        /// <summary>
        /// An estimated count of items in the dictionary.
        /// </summary>
        public int Count => _items.Count;

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
                _items.Add(key, value);
        }

        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            lock (this)
                foreach (var item in items)
                    _items.Add(item.Key, item.Value);
        }

        public void AddOrSet(TKey key, TValue value)
        {
            lock (this)
                _items[key] = value;
        }

        public bool TryAdd(KeyValuePair<TKey, TValue> item) => TryAdd(item.Key, item.Value);
        public bool TryAdd(TKey key, TValue value)
        {
            lock (this)
            {
                if (_items.ContainsKey(key))
                    return false;
                _items.Add(key, value);
                return true;
            }
        }

        public bool TryRemove(KeyValuePair<TKey, TValue> item) => TryRemove(item.Key);

        public bool TryConsume(out KeyValuePair<TKey, TValue> item)
        {
            lock (this)
            {
                if (_items.Count == 0)
                {
                    item = default;
                    return false;
                }

                item = _items.FirstOrDefault();
                _items.Remove(item.Key);
                return true;
            }
        }

        public bool TryRemove(TKey key)
        {
            lock (this)
                return _items.Remove(key);
        }

        public bool ContainsKey(TKey key)
        {
            lock (this)
                return _items.ContainsKey(key);
        }

        public TValue GetOrDefault(TKey key) => TryGetValue(key, out var value) ? value : default(TValue);
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (this)
                return _items.TryGetValue(key, out value);
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
        {
            if (TryGetValue(key, out var value))
                return value;
            value = factory.NotNull(nameof(factory)).Invoke(key);
            lock (this)
            {
                if (_items.TryGetValue(key, out var current))
                    return current;
                _items[key] = value;
                return value;
            }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> GetAndClear()
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
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (Monitor.IsEntered(this))
                return _items.GetEnumerator();
            lock (this)
                return _items.ToList().GetEnumerator();
        }
    }
}