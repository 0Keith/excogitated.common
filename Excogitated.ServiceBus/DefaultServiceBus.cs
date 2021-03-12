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
        private readonly ConcurrentDictionary<Type, IPublisherTransport> _publishers = new();
        private readonly ConcurrentQueue<IConsumerTransport> _consumers = new();
        private readonly AsyncLock _publisherLock = new();
        private readonly AtomicBool _started = new();
        private readonly AtomicBool _stopped = new();
        private readonly IServiceProvider _provider;
        private readonly ConsumerDefinition[] _consumerDefinitions;

        public DefaultServiceBus(IServiceProvider provider)
        {
            _provider = provider;
            _consumerDefinitions = _provider.GetServices<ConsumerDefinition>().ToArray();
        }

        async ValueTask IServiceBus.Publish<T>(T message, CancellationToken cancellationToken)
        {
            var messageType = message.ThrowIfNull(nameof(message)).GetType();
            if (!_publishers.TryGetValue(messageType, out var publisher))
            {
                using (await _publisherLock.EnterAsync())
                {
                    if (!_publishers.TryGetValue(messageType, out publisher))
                    {
                        publisher = _provider.GetRequiredService<IPublisherTransport>();
                        await publisher.StartAsync(new PublisherDefinition
                        {
                            MessageType = messageType
                        }, cancellationToken);
                        _publishers[messageType] = publisher;
                    }
                }
            }
            var serializer = _provider.GetRequiredService<IServiceBusSerializer>();
            var data = serializer.Serialize(message);
            await publisher.Publish(data, cancellationToken);
        }

        public ValueTask StartAsync(CancellationToken cancellationToken) => Task.Run(async () =>
        {
            if (_started.TrySet(true))
                foreach (var definition in _consumerDefinitions)
                {
                    var consumer = _provider.GetRequiredService<IConsumerTransport>();
                    var pipeline = _provider.GetRequiredService<IConsumerPipeline>();
                    await consumer.StartAsync(definition, pipeline, cancellationToken);
                    _consumers.Enqueue(consumer);
                }
        }, cancellationToken).ToValueTask();

        public ValueTask StopAsync(CancellationToken cancellationToken) => Task.Run(async () =>
        {
            if (_started && _stopped.TrySet(true))
                while (_consumers.TryDequeue(out var consumer))
                {
                    await consumer.StopAsync(cancellationToken);
                }
        }, cancellationToken).ToValueTask();
    }
}
