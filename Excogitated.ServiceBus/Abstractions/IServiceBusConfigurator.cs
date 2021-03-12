using Microsoft.Extensions.DependencyInjection;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IServiceBusConfigurator
    {
        IServiceCollection Services { get; }
    }
}