using Excogitated.Extensions;
using MongoDB.Bson.Serialization;
using System;

namespace Excogitated.Mongo
{
    public class DelegateSerializer<T> : IBsonSerializer<T>
    {
        private readonly Func<BsonDeserializationContext, T> _deserialize;
        private readonly Action<BsonSerializationContext, T> _serialize;

        public Type ValueType => typeof(T);

        public DelegateSerializer(Func<BsonDeserializationContext, T> deserialize, Action<BsonSerializationContext, T> serialize)
        {
            _deserialize = deserialize.NotNull(nameof(deserialize));
            _serialize = serialize.NotNull(nameof(serialize));
        }

        public T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) => _deserialize(context);
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value) => _serialize(context, value);
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value) => _serialize(context, (T)value);
        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) => _deserialize(context);
    }
}
