using Excogitated.Mongo;
using Excogitated.ServiceBus.Abstractions;
using Excogitated.Threading;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
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

        public ValueTask DisposeAsync() => StopAsync(default);

        public MongoConsumerTransport(IMongoDatabase database, IServiceBus serviceBus)
        {
            Database = database;
            ServiceBus = serviceBus;
        }

        public async ValueTask StartAsync(ConsumerDefinition consumerDefinition, IConsumerPipeline pipeline, CancellationToken cancellationToken)
        {
            if (_started.TrySet(true))
            {
                var subs = Database.GetCollection<SubscriptionDocument>();
                var queueName = consumerDefinition.ConsumerType.FullName;
                var topicName = consumerDefinition.MessageType.FullName;
                var subExists = await subs.AsQueryable()
                    .Where(d => d.QueueName == queueName && d.TopicName == topicName)
                    .AnyAsync();
                if (!subExists)
                {
                    await subs.ReplaceOneAsync(d => d.QueueName == queueName && d.TopicName == topicName, new TopicDocument
                    {
                        Id = Guid.NewGuid(),
                        QueueName = queueName,
                        TopicName = topicName
                    }, new ReplaceOptions
                    {
                        IsUpsert = true
                    });
                }
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
                        foreach (var message in messages)
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
                    }
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