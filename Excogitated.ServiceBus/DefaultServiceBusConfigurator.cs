using Excogitated.Common.Extensions;
using Excogitated.ServiceBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Excogitated.ServiceBus
{
    internal class DefaultServiceBusConfigurator : IServiceBusConfigurator
    {
        public IServiceCollection Services { get; }

        public DefaultServiceBusConfigurator(IServiceCollection services)
        {
            Services = services.ThrowIfNull(nameof(services));
        }
    }
}