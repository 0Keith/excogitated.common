using Excogitated.ServiceBus.Abstractions;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Azure
{
    internal class AzureServiceBusHostedService : IHostedService
    {
        private readonly IServiceBus _bus;

        public AzureServiceBusHostedService(IServiceBus bus)
        {
            _bus = bus;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _bus.StartAsync(cancellationToken);
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return _bus.StopAsync(cancellationToken);
        }
    }
}
