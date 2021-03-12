using Azure.Messaging.ServiceBus;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Azure.Abstractions
{
    public interface IAzureClientFactory
    {
        Task<ServiceBusProcessor> BuildProcessorAsync(AzureTopologyDefinition topologyDefinition, CancellationToken cancellationToken);
        Task<ServiceBusSender> BuildSenderAsync(AzureTopologyDefinition topologyDefinition, CancellationToken cancellationToken);
    }
}