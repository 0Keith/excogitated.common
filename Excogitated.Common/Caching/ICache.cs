using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Excogitated.Common.Caching
{
    public interface ICache<TKey, TValue>
    {
        /// <summary>
        /// Get item from the cache by key.
        /// </summary>
        ValueTask<TValue> GetAsync(TKey key, ICacheDataProvider<TKey, TValue> dataProvider = null);

        /// <summary>
        /// Get items from the cache by keyword.
        /// </summary>
        ValueTask<List<TValue>> SearchAsync(string keyword, ICacheDataProvider<TKey, TValue> dataProvider = null);
    }

    public interface ICacheDataProvider<TKey, TValue>
    {
        /// <summary>
        /// Called when initially populating the cache and when refreshing it.
        /// </summary>
        Task<IEnumerable<TValue>> GetData();

        /// <summary>
        /// Called for each item in the cache when indexing items.
        /// </summary>
        TKey GetKey(TValue value);

        /// <summary>
        /// Called for each item in the cache when performing a search.
        /// </summary>
        bool SearchData(string keyword, TKey key, TValue value);

        /// <summary>
        /// Called when exceptions occur and are not handled in GetData().
        /// </summary>
        void Log(Exception exception);
    }
}