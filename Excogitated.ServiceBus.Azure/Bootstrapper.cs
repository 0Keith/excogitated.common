using Excogitated.Extensions;
using Excogitated.ServiceBus.Abstractions;
using Excogitated.ServiceBus.Azure.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Excogitated.ServiceBus.Azure
{
    public static class Bootstrapper
    {
        public static IServiceBusConfigurator AddAzureTransport(this IServiceBusConfigurator config)
        {
            return config.ThrowIfNull(nameof(config))
                .AddPublisherTransport<AzurePublisherTransport>()
                .AddConsumerTransport<AzureConsumerTransport>()
                .AddClientFactory<AzureClientFactory>();
        }

        public static IServiceBusConfigurator AddForwardingTopologyBuilder(this IServiceBusConfigurator config, string entityPrefix = null)
        {
            return config.AddTopologyBuilder(new AzureForwardingTopologyBuilder(entityPrefix));
        }

        public static IServiceBusConfigurator AddTopologyBuilder(this IServiceBusConfigurator config, IAzureTopologyBuilder builder)
        {
            config.ThrowIfNull(nameof(config));
            config.Services.AddSingleton(builder);
            return config;
        }

        public static IServiceBusConfigurator AddTopologyBuilder<T>(this IServiceBusConfigurator config)
            where T : class, IAzureTopologyBuilder
        {
            config.ThrowIfNull(nameof(config));
            config.Services.AddSingleton<IAzureTopologyBuilder, T>();
            return config;
        }

        public static IServiceBusConfigurator AddClientFactory<T>(this IServiceBusConfigurator config)
            where T : class, IAzureClientFactory
        {
            config.ThrowIfNull(nameof(config));
            config.Services.AddSingleton<IAzureClientFactory, T>();
            return config;
        }
    }
}
