using Excogitated.Extensions;
using Excogitated.ServiceBus.Abstractions;
using Excogitated.Threading;
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
        private readonly ConcurrentDictionary<Type, IPublisherTransport> _publisherTransports = new();
        private readonly ConcurrentQueue<IConsumerTransport> _consumerTransports = new();
        private readonly AsyncLock _publisherLock = new();
        private readonly AtomicBool _started = new();
        private readonly AtomicBool _stopped = new();

        public IServiceProvider Provider { get; }
        public IConcurrencyLimiter ConcurrencyLimiter { get; }

        public DefaultServiceBus(IServiceProvider provider, IConcurrencyLimiter concurrencyLimiter)
        {
            Provider = provider;
            ConcurrencyLimiter = concurrencyLimiter;
        }

        async ValueTask IServiceBus.Publish<T>(T message, CancellationToken cancellationToken)
        {
            var messageType = message.ThrowIfNull(nameof(message)).GetType();
            if (!_publisherTransports.TryGetValue(messageType, out var transport))
            {
                using (await _publisherLock.EnterAsync())
                {
                    if (!_publisherTransports.TryGetValue(messageType, out transport))
                    {
                        transport = Provider.GetRequiredService<IPublisherTransport>();
                        await transport.StartAsync(new PublisherDefinition
                        {
                            MessageType = messageType
                        }, cancellationToken);
                        _publisherTransports[messageType] = transport;
                    }
                }
            }
            var serializer = Provider.GetRequiredService<IServiceBusSerializer>();
            var data = serializer.Serialize(message);
            using (await ConcurrencyLimiter.AcquirePublishSlot())
                await transport.Publish(data, cancellationToken);
        }

        public async ValueTask StartAsync(CancellationToken cancellationToken)
        {
            if (_started.TrySet(true))
            {
                var consumerDefinitions = Provider.GetServices<ConsumerDefinition>().ToList();
                foreach (var definition in consumerDefinitions)
                {
                    var pipeline = Provider.GetRequiredService<IConsumerPipeline>();
                    var transport = Provider.GetRequiredService<IConsumerTransport>();
                    await transport.StartAsync(definition, pipeline, cancellationToken);
                    _consumerTransports.Enqueue(transport);
                }
            }
        }

        public async ValueTask StopAsync(CancellationToken cancellationToken)
        {
            if (_started && _stopped.TrySet(true))
            {
                while (_consumerTransports.TryDequeue(out var transport))
                {
                    await transport.StopAsync(cancellationToken);
                }

                foreach (var key in _publisherTransports.Keys)
                {
                    if (_publisherTransports.TryRemove(key, out var transport))
                        await transport.StopAsync(cancellationToken);
                }
            }
        }
    }
}
