using System;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IPublisherTransport
    {
        ValueTask Publish(BinaryData message, CancellationToken cancellationToken);
        ValueTask StartAsync(PublisherDefinition definition, CancellationToken cancellationToken);
        ValueTask StopAsync(CancellationToken cancellationToken);
    }
}
