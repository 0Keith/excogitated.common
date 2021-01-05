using Excogitated.Common.Atomic;
using Excogitated.Common.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Excogitated.Common.Mongo
{
    public static class MongoExtensions
    {
        public static void Register(this IConvention convention) => ConventionRegistry.Register(convention.Name, new ConventionPack { convention }, t => true);
        public static IMongoQueryable<R> Project<T, R>(this IMongoQueryable<T> query, Expression<Func<T, R>> selector) => query?.Select(selector);
        public static IMongoQueryable<T> Match<T>(this IMongoQueryable<T> query, Expression<Func<T, bool>> filter) => query?.Where(filter);

        public static IMongoCollection<T> GetCollection<T>(this IMongoDatabase db, Action<BsonClassMap<T>> classMapInitializer = null)
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
                if (classMapInitializer is null)
                    BsonClassMap.RegisterClassMap<T>();
                else
                    BsonClassMap.RegisterClassMap(classMapInitializer);
            return db.NotNull(nameof(db)).GetCollection<T>(typeof(T).Name);
        }

        public static Task CreateIndexAscendingAsync<T>(this IMongoCollection<T> collection, bool unique, params Expression<Func<T, object>>[] indexFields)
        {
            return collection.CreateIndexAsync(unique, Builders<T>.IndexKeys.Ascending, indexFields);
        }

        public static Task CreateIndexDescendingAsync<T>(this IMongoCollection<T> collection, bool unique, params Expression<Func<T, object>>[] indexFields)
        {
            return collection.CreateIndexAsync(unique, Builders<T>.IndexKeys.Descending, indexFields);
        }

        public static Task CreateIndexAsync<T>(this IMongoCollection<T> collection, bool unique, Func<Expression<Func<T, object>>, IndexKeysDefinition<T>> definition, params Expression<Func<T, object>>[] indexFields)
        {
            collection.NotNull(nameof(collection));
            definition.NotNull(nameof(definition));
            indexFields.NotNull(nameof(indexFields));
            IndexKeysDefinition<T> keys;
            if (indexFields.Length == 1)
                keys = definition.Invoke(indexFields[0]);
            else
                keys = Builders<T>.IndexKeys.Combine(indexFields.Select(definition));
            return collection.Indexes.CreateOneAsync(new CreateIndexModel<T>(keys, new CreateIndexOptions { Unique = unique }));
        }

        private static readonly UpdateOptions _upsert = new UpdateOptions { IsUpsert = true };
        public static Task<UpdateResult> UpsertAsync<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> filter, Func<UpdateDefinitionBuilder<T>, UpdateDefinition<T>> update)
        {
            collection.NotNull(nameof(collection));
            return collection.UpdateOneAsync(filter, update(Builders<T>.Update), _upsert);
        }


        private static readonly AtomicBool _initialized = new AtomicBool();
        public static IMongoDatabase GetDatabase(this MongoStoreConfig config)
        {
            config.NotNull(nameof(config));
            if (_initialized.TrySet(true))
            {
                BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));
                BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
                BsonSerializer.RegisterSerializer(new DelegateSerializer<Date>(c => c.Reader.ReadInt32(), (c, v) => c.Writer.WriteInt32(v)));
                new EnumRepresentationConvention(BsonType.String).Register();
                new IgnoreExtraElementsConvention(true).Register();
            }
            var client = new MongoClient($"mongodb+srv://{config.Username}:{config.Password}@{config.Server}/{config.Database}?retryWrites=true&w=majority&useTLS=true");
            return client.GetDatabase(config.Database);
        }

        public static Task<AppSettingStore> GetAppSettings(this IMongoDatabase database)
        {
            database.NotNull(nameof(database));
            return AppSettingStore.Create(database);
        }
    }
}
