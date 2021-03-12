using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IConsumerTransport
    {
        Task StartAsync(ConsumerDefinition consumerDefinition, IConsumerPipeline pipeline, CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
