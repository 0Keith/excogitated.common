using Azure.Messaging.ServiceBus;
using Excogitated.ServiceBus.Abstractions;
using Excogitated.ServiceBus.Azure.Abstractions;
using Excogitated.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Azure
{
    internal class AzurePublisherTransport : IPublisherTransport, IAsyncDisposable
    {
        private readonly AtomicBool _started = new();
        private readonly AtomicBool _stopped = new();
        private readonly IAzureClientFactory _clientFactory;
        private readonly IAzureTopologyBuilder _topologyBuilder;
        private ServiceBusSender _sender;

        public ValueTask DisposeAsync() => StopAsync(default);

        public AzurePublisherTransport(IAzureClientFactory clientFactory, IAzureTopologyBuilder topologyBuilder)
        {
            _clientFactory = clientFactory;
            _topologyBuilder = topologyBuilder;
        }

        public async ValueTask Publish(BinaryData message, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(new ServiceBusMessage
            {
                Body = message,
            }, cancellationToken);
        }

        public async ValueTask StartAsync(PublisherDefinition definition, CancellationToken cancellationToken)
        {
            if (_started.TrySet(true))
            {
                var topology = _topologyBuilder.BuildFrom(definition);
                _sender = await _clientFactory.BuildSenderAsync(topology, cancellationToken);
            }
        }

        public async ValueTask StopAsync(CancellationToken cancellationToken)
        {
            if (_started && _stopped.TrySet(true))
            {
                await using (_sender)
                    await _sender.CloseAsync(cancellationToken);
            }
        }
    }
}
