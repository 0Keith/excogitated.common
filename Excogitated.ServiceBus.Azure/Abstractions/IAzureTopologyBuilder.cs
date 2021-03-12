using Excogitated.ServiceBus.Abstractions;

namespace Excogitated.ServiceBus.Azure.Abstractions
{
    public interface IAzureTopologyBuilder
    {
        AzureTopologyDefinition BuildFrom(ConsumerDefinition definition);
        AzureTopologyDefinition BuildFrom(PublisherDefinition definition);
    }
}
