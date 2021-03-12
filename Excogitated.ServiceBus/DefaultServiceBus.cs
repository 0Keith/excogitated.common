using Excogitated.Common.Atomic;
using Excogitated.Common.Extensions;
using Excogitated.ServiceBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class DefaultServiceBus : IServiceBus
    {
        private readonly ConcurrentDictionary<Type, IPublisherTransport> _publishers = new ConcurrentDictionary<Type, IPublisherTransport>();
        private readonly ConcurrentQueue<IConsumerTransport> _consumers = new ConcurrentQueue<IConsumerTransport>();
        private readonly AsyncLock _publisherLock = new AsyncLock();
        private readonly IServiceProvider _provider;
        private readonly ConsumerDefinition[] _consumerDefinitions;

        public DefaultServiceBus(IServiceProvider provider)
        {
            _provider = provider;
            _consumerDefinitions = _provider.GetServices<ConsumerDefinition>().ToArray();
        }

        async Task IServiceBus.Publish<T>(T message)
        {
            var messageType = message.ThrowIfNull(nameof(message)).GetType();
            if (!_publishers.TryGetValue(messageType, out var publisher))
            {
                using (await _publisherLock.EnterAsync())
                {
                    if (!_publishers.TryGetValue(messageType, out publisher))
                    {
                        publisher = _provider.GetRequiredService<IPublisherTransport>();
                        await publisher.Configure(new PublisherDefinition
                        {
                            MessageType = messageType
                        });
                        _publishers[messageType] = publisher;
                    }
                }
            }
            var serializer = _provider.GetRequiredService<IServiceBusSerializer>();
            var data = serializer.Serialize(message);
            await publisher.Publish(data);
        }

        Task IServiceBus.StartAsync(CancellationToken cancellationToken) => Task.Run(async () =>
        {
            foreach (var definition in _consumerDefinitions)
            {
                var consumer = _provider.GetRequiredService<IConsumerTransport>();
                var pipeline = _provider.GetRequiredService<IConsumerPipeline>();
                await consumer.StartAsync(definition, pipeline, cancellationToken);
                _consumers.Enqueue(consumer);
            }
        });

        Task IServiceBus.StopAsync(CancellationToken cancellationToken) => Task.Run(async () =>
        {
            while (_consumers.TryDequeue(out var consumer))
            {
                await consumer.StopAsync(cancellationToken);
            }
        });
    }
}
