using Excogitated.Common.Extensions;
using Excogitated.ServiceBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Excogitated.ServiceBus.Azure
{
    public static class Bootstrapper
    {
        public static IServiceBusConfigurator StartHostedServiceBusWithAzureTransport(this IServiceCollection services, params Assembly[] consumerAssemblies)
        {
            services.ThrowIfNull(nameof(services));
            var config = services.AddDefaultServiceBus();
            services.AddSingleton<IAzureTopologyBuilder, AzureForwardingTopologyBuilder>()
                .AddTransient<IConsumerTransport, AzureServiceBusConsumerTransport>()
                .AddTransient<IPublisherTransport, AzureServiceBusPublisherTransport>()
                .AddSingleton<AzureClientFactory>();
            if (consumerAssemblies?.Length > 0)
            {
                config.AddServiceBusConsumers(consumerAssemblies);
            }
            services.AddHostedService<AzureServiceBusHostedService>();
            config.AddServiceBusRetry(new RetryDefinition
            {
                MaxDuration = TimeSpan.FromMinutes(1),
                Interval = TimeSpan.FromSeconds(1),
                Increment = TimeSpan.FromSeconds(1)
            })
                .AddServiceBusRedelivery(new RetryDefinition
                {
                    MaxDuration = TimeSpan.FromDays(14),
                    Interval = TimeSpan.FromMinutes(5),
                    Increment = TimeSpan.FromMinutes(5)
                })
                .AddServiceBusTransaction();
            return config;
        }
    }
}
