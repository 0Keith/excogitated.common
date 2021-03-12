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
        public static IServiceBusConfigurator AddDefaultServiceBus(this IServiceCollection services)
        {
            services.ThrowIfNull(nameof(services));
            services.AddSingleton<IServiceBus, DefaultServiceBus>();
            services.AddSingleton<IServiceBusSerializer, DefaultServiceBusSerializer>();
            services.AddSingleton<IConsumerPipeline, DefaultConsumerPipeline>();
            return new DefaultServiceBusConfigurator(services);
        }

        public static IServiceBusConfigurator AddServiceBusConsumers(this IServiceBusConfigurator config, params Assembly[] consumerAssemblies)
        {
            config.ThrowIfNull(nameof(config));
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
                        config.Services.AddSingleton(new ConsumerDefinition(consumerType, consumerInterface, messageType, pipelineType));
                        config.Services.AddScoped(consumerType);
                        config.Services.AddScoped(pipelineType);
                    }
                }
            }
            return config;
        }

        public static IServiceBusConfigurator AddServiceBusRedelivery(this IServiceBusConfigurator config, RetryDefinition retryDefinition)
        {
            config.ThrowIfNull(nameof(config));
            config.ThrowIfNull(nameof(retryDefinition));
            config.Services.AddSingleton<IRedeliveryPipelineFactory>(new DefaultRedeliveryPipelineFactory(retryDefinition));
            return config;
        }

        public static IServiceBusConfigurator AddServiceBusRetry(this IServiceBusConfigurator config, RetryDefinition retryDefinition)
        {
            config.ThrowIfNull(nameof(config));
            config.Services.AddSingleton<IRetryPipelineFactory>(new DefaultRetryPipelineFactory(retryDefinition));
            return config;
        }

        public static IServiceBusConfigurator AddServiceBusTransaction(this IServiceBusConfigurator config)
        {
            config.ThrowIfNull(nameof(config));
            config.Services.AddSingleton<ITransactionPipelineFactory, DefaultTransactionPipelineFactory>();
            return config;
        }
    }
}
