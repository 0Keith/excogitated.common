using Azure.Messaging.ServiceBus;
using Excogitated.ServiceBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Azure
{
    internal class AzureServiceBusPublisherTransport : IPublisherTransport
    {
        private readonly TransportSettings _settings;
        private readonly AzureClientFactory _clientFactory;
        private readonly IAzureTopologyBuilder _topologyBuilder;
        private readonly IServiceBus _serviceBus;
        private ServiceBusSender _sender;

        public AzureServiceBusPublisherTransport(TransportSettings settings, AzureClientFactory clientFactory, IAzureTopologyBuilder topologyBuilder, IServiceBus serviceBus)
        {
            _settings = settings;
            _clientFactory = clientFactory;
            _topologyBuilder = topologyBuilder;
            _serviceBus = serviceBus;
        }

        public async Task Configure(PublisherDefinition definition)
        {
            var topology = _topologyBuilder.BuildFrom(definition);
            _sender = await _clientFactory.BuildSenderAsync(topology);
        }

        public async Task Publish(BinaryData message)
        {
            await _sender.SendMessageAsync(new ServiceBusMessage
            {
                Body = message,




            });
        }
    }
}
