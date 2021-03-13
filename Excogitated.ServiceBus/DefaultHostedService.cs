using Excogitated.Extensions;
using Excogitated.ServiceBus.Abstractions;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class DefaultHostedService : IHostedService
    {
        public IServiceBus Bus { get; }

        public DefaultHostedService(IServiceBus bus)
        {
            Bus = bus.ThrowIfNull(nameof(bus));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Bus.StartAsync(cancellationToken).AsTask();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Bus.StopAsync(cancellationToken).AsTask();
        }
    }
}
