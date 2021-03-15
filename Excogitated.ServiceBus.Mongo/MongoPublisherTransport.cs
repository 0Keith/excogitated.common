using Excogitated.Mongo;
using Excogitated.ServiceBus.Abstractions;
using Excogitated.Threading;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Mongo
{
    internal class MongoPublisherTransport : IPublisherTransport, IAsyncDisposable
    {
        private readonly AtomicBool _started = new();
        private readonly AtomicBool _stopped = new();
        private string _topicName;

        public IMongoDatabase Database { get; }
        public IMongoCollection<SubscriptionDocument> Subscriptions { get; }

        public ValueTask DisposeAsync() => StopAsync(default);

        public MongoPublisherTransport(IMongoDatabase database)
        {
            Database = database;
            Subscriptions = Database.GetCollection<SubscriptionDocument>();
        }

        public async ValueTask Publish(BinaryData message, CancellationToken cancellationToken)
        {
            var queueNames = await Subscriptions.AsQueryable()
                .Match(d => d.TopicName == _topicName)
                .Project(d => d.QueueName)
                .ToListAsync(cancellationToken);
            foreach (var queueName in queueNames)
            {
                await Database.GetCollection<MessageDocument>(queueName).InsertOneAsync(new MessageDocument
                {
                    Id = Guid.NewGuid(),
                    Data = message.ToArray(),
                    PublishedAt = DateTimeOffset.Now,
                    MessageType = _topicName
                }, null, cancellationToken);
            }
        }

        public async ValueTask StartAsync(PublisherDefinition definition, CancellationToken cancellationToken)
        {
            if (_started.TrySet(true))
            {
                _topicName = definition.MessageType.FullName;
            }
        }

        public async ValueTask StopAsync(CancellationToken cancellationToken)
        {
            if (_started && _stopped.TrySet(true))
            {
            }
        }
    }
}
