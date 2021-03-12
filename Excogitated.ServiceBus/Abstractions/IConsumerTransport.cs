using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IConsumerTransport
    {
        ValueTask StartAsync(ConsumerDefinition consumerDefinition, IConsumerPipeline pipeline, CancellationToken cancellationToken);
        ValueTask StopAsync(CancellationToken cancellationToken);
    }
}
