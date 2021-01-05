using Excogitated.Common.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Excogitated.Common.Mongo
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
                .SetOnInsert(d => d.Key, key)
                .SetOnInsert(d => d.Id, Guid.NewGuid()));
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

        public async Task<List<AppSettingDocument>> GetAndClear()
        {
            var documents = await _documents.AsQueryable().ToListAsync();
            var ids = documents.Select(d => d.Id).ToList();
            var result = await _documents.DeleteManyAsync(d => ids.Contains(d.Id));
            return result.IsAcknowledged ? documents : null;
        }
    }
}