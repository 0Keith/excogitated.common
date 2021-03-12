using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IServiceBus
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);

        Task Publish<T>(T message) where T : class;
    }
}
