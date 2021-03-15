using Excogitated.Mongo;
using Excogitated.ServiceBus.Abstractions;
using Excogitated.Threading;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Mongo
{
    internal class MongoConsumerTransport : IConsumerTransport, IAsyncDisposable
    {
        private readonly AtomicBool _started = new();
        private readonly AtomicBool _stopped = new();
        private Task _processor;

        public IMongoDatabase Database { get; }
        public IServiceBus ServiceBus { get; }
        public IConcurrencyLimiter ConcurrencyLimiter { get; }

        public ValueTask DisposeAsync() => StopAsync(default);

        public MongoConsumerTransport(IMongoDatabase database, IServiceBus serviceBus, IConcurrencyLimiter concurrencyLimiter)
        {
            Database = database;
            ServiceBus = serviceBus;
            ConcurrencyLimiter = concurrencyLimiter;
        }

        public async ValueTask StartAsync(ConsumerDefinition consumerDefinition, IConsumerPipeline pipeline, CancellationToken cancellationToken)
        {
            if (_started.TrySet(true))
            {
                await BuildSubscription(consumerDefinition);
                var messageType = consumerDefinition.MessageType.FullName;
                var queue = Database.GetCollection<MessageDocument>(consumerDefinition.ConsumerType.FullName);
                _processor = Task.Run(async () =>
                {
                    while (!_stopped)
                    {
                        var now = DateTimeOffset.Now;
                        var messages = await queue.AsQueryable()
                            .Where(d => d.MessageType == messageType)
                            .Where(d => !(d.LockExpiration >= now))
                            .ToListAsync();
                        if (messages.Count == 0)
                            await Task.Delay(1000);
                        await Task.WhenAll(messages.Select(async message =>
                        {
                            if (!_stopped)
                            {
                                var update = Builders<MessageDocument>.Update.Set(d => d.LockExpiration, now.AddMinutes(1));
                                var result = await queue.UpdateOneAsync(d => d.Id == message.Id && !(d.LockExpiration >= now), update);
                                if (result.ModifiedCount > 0)
                                {
                                    var context = new MongoConsumerContext(ServiceBus);
                                    var data = new BinaryData(message.Data);
                                    var task = pipeline.Execute(context, data, consumerDefinition).AsTask();
                                    while (!task.IsCompleted)
                                    {
                                        await Task.WhenAny(task, Task.Delay(30000));
                                        if (!task.IsCompleted)
                                        {
                                            now = DateTimeOffset.Now;
                                            update = Builders<MessageDocument>.Update.Set(d => d.LockExpiration, now.AddMinutes(1));
                                            result = await queue.UpdateOneAsync(d => d.Id == message.Id, update);
                                        }
                                    }
                                    await queue.DeleteOneAsync(d => d.Id == message.Id);
                                }
                            }
                        }));
                    }
                });
            }
        }

        private async Task BuildSubscription(ConsumerDefinition consumerDefinition)
        {
            var topicName = consumerDefinition.MessageType.FullName;
            var queueName = consumerDefinition.ConsumerType.FullName;
            var subscriptions = Database.GetCollection<SubscriptionDocument>();
            var subscriptionExists = await subscriptions.AsQueryable()
                .Where(d => d.TopicName == topicName)
                .Where(d => d.QueueName == queueName)
                .AnyAsync();
            if (!subscriptionExists)
            {
                await subscriptions.ReplaceOneAsync(d => d.TopicName == topicName && d.QueueName == queueName, new SubscriptionDocument
                {
                    Id = Guid.NewGuid(),
                    TopicName = topicName,
                    QueueName = queueName
                }, new ReplaceOptions
                {
                    IsUpsert = true
                });
            }
        }

        public async ValueTask StopAsync(CancellationToken cancellationToken)
        {
            if (_started && _stopped.TrySet(true))
            {
                await _processor;
            }
        }

    }
}