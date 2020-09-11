using System;
using System.Collections.Generic;

namespace Excogitated.Common.Atomic.Collections
{
    public interface IAtomicDictionary<TKey, TValue> : IAtomicCollection<KeyValuePair<TKey, TValue>>
    {
        TValue this[TKey key] { get; set; }
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }

        void Add(TKey key, TValue value);
        void AddOrSet(TKey key, TValue value);
        bool TryAdd(TKey key, TValue value);

        bool ContainsKey(TKey key);

        bool TryGetValue(TKey key, out TValue value);
        TValue GetOrDefault(TKey key);
        TValue GetOrAdd(TKey key, Func<TKey, TValue> factory);
        bool TryRemove(TKey key);
    }

    public static class ExtAtomicDictionary
    {
        public static TValue GetOrAdd<TKey, TValue>(this IAtomicDictionary<TKey, TValue> source, TKey key) where TValue : new() => source.GetOrAdd(key, k => new TValue());
    }
}