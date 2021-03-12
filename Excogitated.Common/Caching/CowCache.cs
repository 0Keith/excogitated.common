using Excogitated.Common.Atomic;
using Excogitated.Common.Atomic.Collections;
using Excogitated.Common.Extensions;
using Excogitated.Common.Json;
using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Caching
{
    /// <summary>
    /// Settings for Cache.
    /// </summary>
    public struct CowCacheSettings
    {
        /// <summary>
        /// Interval between refreshing cache from factory. Null will disable refreshing and factory will only be called once.
        /// </summary>
        public TimeSpan? RefreshInterval { get; set; }

        /// <summary>
        /// Interval between refreshing cache from factory after an Exception occurs. If null will default to RefreshInterval.
        /// </summary>
        public TimeSpan? RefreshIntervalOnException { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ICacheValueFactory<TValue> : ICacheValueFactory<Type, TValue> { }

    /// <summary>
    /// 
    /// </summary>
    public interface ICacheValueFactory<TKey, TValue>
    {
        /// <summary>
        /// Get a value from the Factory by Key.
        /// </summary>
        ValueTask<TValue> GetValue(TKey key, CacheResult<TValue> previousValue);
    }

    /// <summary>
    /// Source of Value in cache.
    /// </summary>
    public enum CacheSource
    {
        /// <summary>
        /// No Value was able to be obtained from cache.
        /// </summary>
        None,

        /// <summary>
        /// Value was obtained from in memory cache.
        /// </summary>
        Cache,

        /// <summary>
        /// Cached Value was expired and could not be refreshed.
        /// </summary>
        Expired,

        /// <summary>
        /// Value was obtained from Factory.
        /// </summary>
        Factory,

        /// <summary>
        /// Value was obtained from Backup Factory.
        /// </summary>
        BackupFactory,
    }

    /// <summary>
    /// Result from Cache containing Value and source.
    /// </summary>
    public struct CacheResult<T>
    {
        /// <summary>
        /// Value from Cache.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Value source.
        /// </summary>
        public CacheSource Source { get; set; }

        /// <summary>
        /// Exception that occurred while obtaining Value from Factory or null.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Json serialization of CacheResult
        /// </summary>
        public override string ToString()
        {
            return Jsonizer.Serialize(this, true);
        }
    }

    /// <summary>
    /// Copy on Write cache intended for highest possible throughput while still maintaining thread safety.
    /// </summary>
    public class CowCache
    {
        private class Context
        {
            public AsyncLock Lock { get; } = new AsyncLock();
            public DateTimeOffset NextRefresh { get; private set; }
            public object Value { get; private set; }
            public CacheSource Source { get; private set; }

            public void SetValue(object value, CacheSource source, TimeSpan? refreshInterval)
            {
                Value = value;
                Source = source;
                if (refreshInterval is null)
                    NextRefresh = DateTimeOffset.MaxValue;
                else
                {
                    var now = DateTimeOffset.Now;
                    var ticks = now.Ticks + refreshInterval.Value.Ticks;
                    if (ticks > DateTimeOffset.MaxValue.Ticks)
                        NextRefresh = DateTimeOffset.MaxValue;
                    else if (ticks < DateTimeOffset.MinValue.Ticks)
                        NextRefresh = DateTimeOffset.MinValue;
                    else
                        NextRefresh = now.Add(refreshInterval.Value);
                }
            }
        }

        private readonly CowDictionary<object, Context> _contexts = new();

        /// <summary>
        /// The cache Settings
        /// </summary>
        public CowCacheSettings Settings { get; }

        /// <summary>
        /// Creates a new instance of the cache with the specified settings.
        /// </summary>
        public CowCache(CowCacheSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// Gets value from cache by type or retrieves from the factory and inserts into the cache.
        /// </summary>
        public ValueTask<CacheResult<TValue>> GetAsync<TValue>(ICacheValueFactory<TValue> factory, ICacheValueFactory<TValue> backupFactory = null)
        {
            if (backupFactory is null)
                return GetAsync<TValue>(factory.GetValue);
            return GetAsync<TValue>(factory.GetValue, backupFactory.GetValue);
        }

        /// <summary>
        /// Gets value from cache with the specified key or retrieves from the factory and inserts into the cache.
        /// </summary>
        public ValueTask<CacheResult<TValue>> GetAsync<TKey, TValue>(TKey key, ICacheValueFactory<TKey, TValue> factory, ICacheValueFactory<TKey, TValue> backupFactory = null)
        {
            if (backupFactory is null)
                return GetAsync<TKey, TValue>(key, factory.GetValue);
            return GetAsync<TKey, TValue>(key, factory.GetValue, backupFactory.GetValue);
        }

        /// <summary>
        /// Gets value from cache by type or retrieves from the factory and inserts into the cache.
        /// </summary>
        public ValueTask<CacheResult<TValue>> GetAsync<TValue>(
            Func<Type, CacheResult<TValue>, ValueTask<TValue>> valueFactory,
            Func<Type, CacheResult<TValue>, ValueTask<TValue>> backupFactory = null)
        {
            return GetAsync(typeof(TValue), valueFactory, backupFactory);
        }

        /// <summary>
        /// Gets value from cache with the specified key or retrieves from the factory and inserts into the cache.
        /// </summary>
        public async ValueTask<CacheResult<TValue>> GetAsync<TKey, TValue>(TKey key,
            Func<TKey, CacheResult<TValue>, ValueTask<TValue>> valueFactory,
            Func<TKey, CacheResult<TValue>, ValueTask<TValue>> backupFactory = null)
        {
            // get or create new context from cache
            var context = _contexts.GetOrAdd(key, k => new Context());

            // return cached value
            var now = DateTimeOffset.Now;
            if (now < context.NextRefresh)
                if (context.Value is TValue value)
                    return new CacheResult<TValue>
                    {
                        Value = value,
                        Source = CacheSource.Cache
                    };

            // check for null after cache miss to avoid unnecessary check
            valueFactory.NotNull(nameof(valueFactory));

            // cache missed, obtain lock for context
            using (await context.Lock.EnterAsync())
            {
                // return cached value if context has been updated while lock was being obtained
                if (now < context.NextRefresh)
                    if (context.Value is TValue value)
                        return new CacheResult<TValue>
                        {
                            Value = value,
                            Source = CacheSource.Cache
                        };

                // get previous value if it exists
                var previousResult = context.Value is TValue previousValue ? new CacheResult<TValue>
                {
                    Value = previousValue,
                    Source = context.Source
                } : default;
                try
                {
                    // invoke value factory with previous value if it exists
                    var data = await valueFactory(key, previousResult);

                    // set cached value and return it
                    context.SetValue(data, CacheSource.Factory, Settings.RefreshInterval);
                    return new CacheResult<TValue>
                    {
                        Value = data,
                        Source = CacheSource.Factory
                    };
                }
                catch (Exception e)
                {
                    var refreshInterval = Settings.RefreshIntervalOnException ?? Settings.RefreshInterval;
                    // use backup factory if it exists
                    if (backupFactory is object)
                    {
                        try
                        {
                            var data = await backupFactory(key, previousResult);
                            context.SetValue(data, CacheSource.BackupFactory, refreshInterval);
                            return new CacheResult<TValue>
                            {
                                Value = data,
                                Source = CacheSource.BackupFactory,
                                Exception = e
                            };
                        }
                        catch (Exception be)
                        {
                            e = new AggregateException(e, be);
                        }
                    }

                    // reset cached value and return both the exception and previous value
                    context.SetValue(context.Value, context.Source, refreshInterval);
                    return new CacheResult<TValue>
                    {
                        Value = context.Value is TValue v ? v : default,
                        Source = context.Source,
                        Exception = e
                    };
                }
            }
        }
    }
}