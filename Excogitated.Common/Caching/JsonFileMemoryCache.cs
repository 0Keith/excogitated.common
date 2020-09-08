using Excogitated.Common.Atomic;
using Excogitated.Common.Atomic.Collections;
using Excogitated.Common.Extensions;
using Excogitated.Common.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common.Caching
{
    public struct JsonFileMemoryCacheSettings
    {
        /// <summary>
        /// Interval between calls to ICacheDataProvider.GetData() to refresh cache. Null will disable refreshing and ICacheDataProvider.GetData() will only be called once.
        /// </summary>
        public TimeSpan? RefreshInterval { get; set; }

        /// <summary>
        /// Interval between calls to ICacheDataProvider.GetData() when an exception occurs. If null will default to RefreshInterval.
        /// </summary>
        public TimeSpan? RefreshIntervalOnException { get; set; }

        /// <summary>
        /// If disabled, all searches will be cached until application is restarted. If enabled, search cache will be cleared after each refresh.
        /// </summary>
        public bool ClearSearchCacheOnRefresh { get; set; }

        /// <summary>
        /// The file path to store the cache to after it is retrieved. A RefreshInterval must be specified if FilePath is specified.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// The preferred time to refresh data in the cache.
        /// </summary>
        public DateTimeOffset PreferredCacheRefreshTime { get; set; }
    }

    /// <summary>
    /// In-memory cache that can be persisted to a Json file.
    /// </summary>
    public class JsonFileMemoryCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly CowDictionary<string, List<TValue>> _searches = new CowDictionary<string, List<TValue>>();
        private readonly CowDictionary<TKey, TValue> _itemsByKey = new CowDictionary<TKey, TValue>();
        private readonly CowList<TValue> _items = new CowList<TValue>();
        private readonly AsyncLock _refreshLock = new AsyncLock();

        private readonly JsonFileMemoryCacheSettings _settings;
        private readonly ICacheDataProvider<TKey, TValue> _dataProvider;
        private DateTimeOffset _nextRefresh;

        /// <summary>
        /// Create a new instance of JsonFileMemoryCache.
        /// </summary>
        /// <param name="dataProvider">Must be specified in constructor or on method calls.</param>
        public JsonFileMemoryCache(JsonFileMemoryCacheSettings settings, ICacheDataProvider<TKey, TValue> dataProvider = null)
        {
            _settings = settings;
            _dataProvider = dataProvider;
        }

        /// <summary>
        /// Gets an item from the cache by key. All available items are indexed after initial call.
        /// </summary>
        /// <param name="dataProvider">Must be specified in constructor or on method calls.</param>
        public async ValueTask<TValue> GetAsync(TKey key, ICacheDataProvider<TKey, TValue> dataProvider = null)
        {
            await TryRefreshData(dataProvider);
            return _itemsByKey.GetOrDefault(key);
        }

        /// <summary>
        /// Search items in cache by keyword. Results will be indexed by keyword.
        /// </summary>
        /// <param name="dataProvider">Must be specified in constructor or on method calls.</param>
        public async ValueTask<List<TValue>> SearchAsync(string keyword, ICacheDataProvider<TKey, TValue> dataProvider = null)
        {
            await TryRefreshData(dataProvider);
            keyword.NotNullOrWhitespace(nameof(keyword));
            if (_searches.TryGetValue(keyword, out var values))
                return values;
            dataProvider = (dataProvider ?? _dataProvider).NotNull(nameof(dataProvider));
            return _searches.GetOrAdd(keyword, k => _itemsByKey.Where(p => dataProvider.SearchData(k, p.Key, p.Value)).Select(p => p.Value).ToList());
        }

        private async ValueTask TryRefreshData(ICacheDataProvider<TKey, TValue> dataProvider)
        {
            var now = DateTimeOffset.Now;
            if (now >= _nextRefresh)
                using (await _refreshLock.EnterAsync())
                    if (now >= _nextRefresh)
                        try
                        {
                            dataProvider = (dataProvider ?? _dataProvider).NotNull(nameof(dataProvider));
                            if (_nextRefresh == DateTimeOffset.MinValue && _settings.RefreshInterval.HasValue && _settings.FilePath is string)
                            {
                                var path = $"{_settings.FilePath.TrimEnd('/', '\\')}.zip";
                                var file = new FileInfo(path);
                                if (file.Exists && file.LastWriteTimeUtc > now.Subtract(_settings.RefreshInterval.Value).UtcDateTime)
                                {
                                    using var zip = ZipFile.OpenRead(file.FullName);
                                    using var stream = zip.GetEntry("items.json").Open();
                                    var items = await Jsonizer.DeserializeAsync<List<TValue>>(stream);
                                    _items.AddRange(items);
                                    _itemsByKey.AddRange(_items.Select(i => KeyValuePair.Create(dataProvider.GetKey(i), i)));
                                    return;
                                }
                            }

                            var data = await dataProvider.GetData().OrDefault();
                            if (data is object)
                            {
                                _items.ClearAndAdd(data);
                                _itemsByKey.ClearAndAdd(_items.Select(i => KeyValuePair.Create(dataProvider.GetKey(i), i)));
                                if (_settings.ClearSearchCacheOnRefresh)
                                    _searches.Clear();
                                else
                                {
                                    var searches = new Dictionary<string, List<TValue>>();
                                    foreach (var key in _searches.Keys)
                                        searches[key] = _itemsByKey.Where(p => dataProvider.SearchData(key, p.Key, p.Value)).Select(p => p.Value).ToList();
                                    _searches.ClearAndAdd(searches);
                                }

                                if (_settings.FilePath is string)
                                {
                                    var path = $"{_settings.FilePath.TrimEnd('/', '\\')}.zip";
                                    var file = new FileInfo(path);
                                    await file.Directory.CreateStrongAsync();
                                    using var zip = ZipFile.Open(file.FullName, ZipArchiveMode.Update);
                                    using var stream = zip.CreateEntry("items.json").Open();
                                    await Jsonizer.SerializeAsync(_items, stream, true);
                                }
                            }

                            now = DateTimeOffset.Now;
                            var nextTicks = now.Ticks.ToDouble() + (_settings.RefreshInterval ?? TimeSpan.MaxValue).Ticks;
                            _nextRefresh = nextTicks <= DateTimeOffset.MaxValue.Ticks && _settings.RefreshInterval.HasValue
                                ? now.Add(_settings.RefreshInterval.Value)
                                : DateTimeOffset.MaxValue;
                        }
                        catch (Exception e)
                        {
                            if (_nextRefresh == DateTimeOffset.MaxValue)
                                throw;
                            dataProvider.Log(e);
                            now = DateTimeOffset.Now;
                            var nextTicks = now.Ticks.ToDouble() + (_settings.RefreshIntervalOnException ?? _settings.RefreshInterval ?? TimeSpan.MaxValue).Ticks;
                            _nextRefresh = DateTimeOffset.MaxValue;
                            if (nextTicks <= DateTimeOffset.MaxValue.Ticks)
                                if (_settings.RefreshIntervalOnException.HasValue)
                                    _nextRefresh = now.Add(_settings.RefreshIntervalOnException.Value);
                                else if (_settings.RefreshInterval.HasValue)
                                    _nextRefresh = now.Add(_settings.RefreshInterval.Value);
                        }
        }
    }
}