using Excogitated.Common.Extensions;
using Excogitated.ServiceBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Excogitated.ServiceBus.Azure
{
    public static class Bootstrapper
    {
        public static IServiceCollection StartHostedServiceBusWithAzureTransport(this IServiceCollection services, params Assembly[] consumerAssemblies)
        {
            services.ThrowIfNull(nameof(services));
            services.AddSingleton<IAzureTopologyBuilder, AzureForwardingTopologyBuilder>();
            services.AddTransient<IConsumerTransport, AzureServiceBusConsumerTransport>();
            services.AddTransient<IPublisherTransport, AzureServiceBusPublisherTransport>();
            services.AddSingleton<AzureClientFactory>();
            if (consumerAssemblies?.Length > 0)
            {
                services.AddServiceBusConsumers(consumerAssemblies);
            }
            services.AddDefaultServiceBus();
            services.AddHostedService<AzureServiceBusHostedService>();
            services.AddServiceBusRetry(new RetryDefinition
            {
                MaxDuration = TimeSpan.FromMinutes(1),
                Interval = TimeSpan.FromSeconds(1),
                Increment = TimeSpan.FromSeconds(1)
            });
            services.AddServiceBusRedelivery(new RetryDefinition
            {
                MaxDuration = TimeSpan.FromDays(14),
                Interval = TimeSpan.FromMinutes(5),
                Increment = TimeSpan.FromMinutes(5)
            });
            services.AddServiceBusTransaction();
            return services;
        }
    }
}
