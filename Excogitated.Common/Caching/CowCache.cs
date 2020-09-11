using Excogitated.Common.Atomic;
using Excogitated.Common.Atomic.Collections;
using Excogitated.Common.Extensions;
using Excogitated.Common.Json;
using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Caching
{
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

        /// <summary>
        /// The preferred time of day to refresh cache from factory.
        /// </summary>
        //public DateTimeOffset PreferredRefreshTimeOfDay { get; set; }
    }

    public interface ICacheValueFactory<TValue> : ICacheValueFactory<Type, TValue> { }

    public interface ICacheValueFactory<TKey, TValue>
    {
        Task<TValue> GetValue(TKey key, CacheResult<TValue> result);
    }

    public struct CacheResult<T>
    {
        public T Value { get; set; }
        public bool Success { get; set; }
        public bool FromCache { get; set; }
        public bool FromFactory { get; set; }
        public Exception Exception { get; set; }

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

            public void SetValue(object value, TimeSpan? refreshInterval)
            {
                Value = value;
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

        private readonly CowDictionary<object, Context> _contexts = new CowDictionary<object, Context>();

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
        public ValueTask<CacheResult<TValue>> GetAsync<TValue>(ICacheValueFactory<TValue> factory)
        {
            return GetAsync<TValue>(factory.GetValue);
        }

        /// <summary>
        /// Gets value from cache by type or retrieves from the factory and inserts into the cache.
        /// </summary>
        public ValueTask<CacheResult<TValue>> GetAsync<TValue>(Func<Type, CacheResult<TValue>, Task<TValue>> valueFactory)
        {
            return GetAsync(typeof(TValue), valueFactory);
        }

        /// <summary>
        /// Gets value from cache with the specified key or retrieves from the factory and inserts into the cache.
        /// </summary>
        public ValueTask<CacheResult<TValue>> GetAsync<TKey, TValue>(TKey key, ICacheValueFactory<TKey, TValue> factory)
        {
            return GetAsync<TKey, TValue>(key, factory.GetValue);
        }

        /// <summary>
        /// Gets value from cache with the specified key or retrieves from the factory and inserts into the cache.
        /// </summary>
        public async ValueTask<CacheResult<TValue>> GetAsync<TKey, TValue>(TKey key, Func<TKey, CacheResult<TValue>, Task<TValue>> valueFactory)
        {
            // get or create new context from cache
            var context = _contexts.GetOrAdd(key, k => new Context());

            // return cached value
            var now = DateTimeOffset.Now;
            if (now > context.NextRefresh)
                if (context.Value is TValue value)
                    return new CacheResult<TValue>
                    {
                        Value = value,
                        Success = true,
                        FromCache = true
                    };

            // cache missed, obtain lock for context
            using (await context.Lock.EnterAsync())
            {
                // return cached value if context has been updated while lock was being obtained
                if (now < context.NextRefresh)
                    if (context.Value is TValue value)
                        return new CacheResult<TValue>
                        {
                            Value = value,
                            Success = true,
                            FromCache = true
                        };
                try
                {
                    // invoke value factory with previous value if it exists
                    var previousResult = context.Value is TValue previousValue ? new CacheResult<TValue>
                    {
                        Value = previousValue,
                        Success = true,
                        FromCache = true
                    } : default;
                    var data = await (valueFactory?.Invoke(key, previousResult)).OrDefault();
                    if (data is TValue factoryValue)
                    {
                        // set cached value and return it
                        context.SetValue(data, Settings.RefreshInterval);
                        return new CacheResult<TValue>
                        {
                            Value = factoryValue,
                            Success = true,
                            FromFactory = false
                        };
                    }
                }
                catch (Exception e)
                {
                    // reset cached value and return both the exception and previous value
                    context.SetValue(context.Value, Settings.RefreshIntervalOnException ?? Settings.RefreshInterval);
                    return new CacheResult<TValue>
                    {
                        Value = context.Value is TValue v ? v : default,
                        Exception = e
                    };
                }

                // this would only happen if the factory returns a null
                return default;
            }
        }
    }
}