using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IServiceBus
    {
        ValueTask Publish<T>(T message, CancellationToken cancellationToken = default) where T : class;
        ValueTask StartAsync(CancellationToken cancellationToken);
        ValueTask StopAsync(CancellationToken cancellationToken);
    }
}
