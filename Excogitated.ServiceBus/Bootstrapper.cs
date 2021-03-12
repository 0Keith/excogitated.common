using Excogitated.Common.Extensions;
using Excogitated.ServiceBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Excogitated.ServiceBus
{
    public static partial class Bootstrapper
    {
        public static IServiceCollection AddDefaultServiceBus(this IServiceCollection services)
        {
            services.ThrowIfNull(nameof(services));
            services.AddSingleton<IServiceBus, DefaultServiceBus>();
            services.AddSingleton<IServiceBusSerializer, DefaultServiceBusSerializer>();
            services.AddSingleton<IConsumerPipeline, DefaultConsumerPipeline>();
            return services;
        }

        public static IServiceCollection AddServiceBusConsumers(this IServiceCollection services, params Assembly[] consumerAssemblies)
        {
            services.ThrowIfNull(nameof(services));
            if (consumerAssemblies is null)
            {
                consumerAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
            var consumerTypes = consumerAssemblies.SelectMany(a => a.DefinedTypes)
                .Where(t => typeof(IConsumer).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .Select(t => t.AsType())
                .ToList();
            var consumerEmptyInterface = typeof(IConsumer<>);
            foreach (var consumerType in consumerTypes)
            {
                foreach (var consumerInterface in consumerType.GetInterfaces())
                {
                    if (consumerInterface.GenericTypeArguments.Length > 0 && consumerEmptyInterface == consumerInterface.GetGenericTypeDefinition())
                    {
                        var messageType = consumerInterface.GenericTypeArguments[0];
                        var pipelineType = typeof(DefaultDeserializerPipeline<,>).MakeGenericType(consumerType, messageType);
                        services.AddSingleton(new ConsumerDefinition(consumerType, consumerInterface, messageType, pipelineType));
                        services.AddScoped(consumerType);
                        services.AddScoped(pipelineType);
                    }
                }
            }
            return services;
        }

        public static IServiceCollection AddServiceBusRedelivery(this IServiceCollection services, RetryDefinition retryDefinition)
        {
            services.ThrowIfNull(nameof(services));
            services.ThrowIfNull(nameof(retryDefinition));
            services.AddSingleton<IRedeliveryPipelineFactory>(new DefaultRedeliveryPipelineFactory(retryDefinition));
            return services;
        }

        public static IServiceCollection AddServiceBusRetry(this IServiceCollection services, RetryDefinition retryDefinition)
        {
            services.ThrowIfNull(nameof(services));
            services.AddSingleton<IRetryPipelineFactory>(new DefaultRetryPipelineFactory(retryDefinition));
            return services;
        }

        public static IServiceCollection AddServiceBusTransaction(this IServiceCollection services)
        {
            services.ThrowIfNull(nameof(services));
            services.AddSingleton<ITransactionPipelineFactory, DefaultTransactionPipelineFactory>();
            return services;
        }
    }
}
