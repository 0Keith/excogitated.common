using Excogitated.Common.Extensions;
using Excogitated.ServiceBus.Abstractions;
using Excogitated.ServiceBus.Azure.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Excogitated.ServiceBus.Azure
{
    public static class Bootstrapper
    {
        public static IServiceBusConfigurator StartHostedServiceBusWithAzureTransport(this IServiceCollection services, params Assembly[] consumerAssemblies)
        {
            var config = services.ThrowIfNull(nameof(services))
                .AddDefaultServiceBus();
            if (consumerAssemblies?.Length > 0)
            {
                config.AddConsumers(consumerAssemblies);
            }
            return config.AddHostedServiceBus()
                .AddTopologyBuilder<AzureForwardingTopologyBuilder>()
                .AddPublisherTransport<AzurePublisherTransport>()
                .AddConsumerTransport<AzureConsumerTransport>()
                .AddClientFactory<AzureClientFactory>()
                .AddConsumerTransaction()
                .AddConsumerRetry(new RetryDefinition
                {
                    MaxDuration = TimeSpan.FromMinutes(1),
                    Interval = TimeSpan.FromSeconds(1),
                    Increment = TimeSpan.FromSeconds(1)
                })
                .AddConsumerRedelivery(new RetryDefinition
                {
                    MaxDuration = TimeSpan.FromDays(14),
                    Interval = TimeSpan.FromMinutes(5),
                    Increment = TimeSpan.FromMinutes(5)
                });
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
