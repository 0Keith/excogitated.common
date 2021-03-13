using Excogitated.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Excogitated.Mongo
{
    public class AppSettingStore
    {
        public static async Task<AppSettingStore> Create(IMongoDatabase db)
        {
            var documents = db.GetCollection<AppSettingDocument>();
            await documents.CreateIndexAscendingAsync(true, d => d.Key);
            return new AppSettingStore(documents);
        }

        private readonly IMongoCollection<AppSettingDocument> _documents;

        private AppSettingStore(IMongoCollection<AppSettingDocument> documents)
        {
            _documents = documents;
        }

        public Task<long> GetCount() => _documents.EstimatedDocumentCountAsync();

        public Task SetAsync<T>(string key, T value)
        {
            return _documents.UpsertAsync(d => d.Key == key, b => b.Set(d => d.Value, value)
                .SetOnInsert(d => d.Id, Guid.NewGuid())
                .SetOnInsert(d => d.Key, key));
        }

        public async Task<T> GetAsync<T>([CallerMemberName] string key = null)
        {
            var value = await _documents.AsQueryable()
                .Where(d => d.Key == key)
                .Select(d => d.Value)
                .FirstOrDefaultAsync();
            return value is T converted ? converted : default;
        }

        public async Task<long> DeleteAsync(string key)
        {
            var result = await _documents.DeleteOneAsync(d => d.Key == key);
            return result.IsAcknowledged ? result.DeletedCount : 0;
        }

        public async Task<List<AppSettingDocument>> GetAndClearAsync()
        {
            var documents = await _documents.AsQueryable().ToListAsync();
            var ids = documents.Select(d => d.Id).ToList();
            var result = await _documents.DeleteManyAsync(d => ids.Contains(d.Id));
            return result.IsAcknowledged ? documents : null;
        }

        public async Task ClearAsync()
        {
            var result = await _documents.DeleteManyAsync(d => true);
            if (!result.IsAcknowledged)
                throw new Exception("Operation Failed.");
        }
    }

    public class AppSettingStore<T>
    {
        public static async Task<AppSettingStore<T>> Create(IMongoDatabase db)
        {
            var store = await AppSettingStore.Create(db);
            return new AppSettingStore<T>(store);
        }

        private readonly AppSettingStore _store;

        private AppSettingStore(AppSettingStore store)
        {
            _store = store;
        }

        public Task<long> GetCount() => _store.GetCount();

        public Task SetAsync<V>(Expression<Func<T, V>> property, V value) => _store.SetAsync(property.GetFullName(), value);

        public Task<V> GetAsync<V>(Expression<Func<T, V>> property) => _store.GetAsync<V>(property.GetFullName());

        public Task<long> DeleteAsync<V>(Expression<Func<T, V>> property) => _store.DeleteAsync(property.GetFullName());

        public Task ClearAsync() => _store.ClearAsync();
    }
}