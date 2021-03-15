using Excogitated.Extensions;
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
            services.AddSingleton<IServiceBusSerializer, DefaultSerializer>();
            services.AddSingleton<IConsumerPipeline, DefaultConsumerPipeline>();
            return new DefaultServiceBusConfigurator(services)
                .AddConcurrencyLimiter(new());
        }

        public static IServiceBusConfigurator AddMemoryTransport(this IServiceBusConfigurator config)
        {
            return config.ThrowIfNull(nameof(config))
                .AddConsumerTransport<DefaultMemoryTransport>()
                .AddPublisherTransport<DefaultMemoryTransport>();
        }

        public static IServiceBusConfigurator AddConsumer<T>(this IServiceBusConfigurator config) where T : IConsumer
        {
            return config.ThrowIfNull(nameof(config))
                .AddConsumer(typeof(T));
        }

        public static IServiceBusConfigurator AddConsumer(this IServiceBusConfigurator config, Type consumerType)
        {
            config.ThrowIfNull(nameof(config));
            consumerType.ThrowIfNull(nameof(consumerType));
            if (!typeof(IConsumer).IsAssignableFrom(consumerType))
                throw new ArgumentException($"Consumer must implement {typeof(IConsumer<>).FullName}");
            if (!consumerType.IsClass)
                throw new ArgumentException($"Consumer must be reference type.");
            if (consumerType.IsAbstract)
                throw new ArgumentException($"Consumer must not be abstract or static.");

            var consumerEmptyInterface = typeof(IConsumer<>);
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
            return config;
        }

        public static IServiceBusConfigurator AddConsumers(this IServiceBusConfigurator config, params Assembly[] consumerAssemblies)
        {
            config.ThrowIfNull(nameof(config));
            if (consumerAssemblies is null || consumerAssemblies.Length == 0)
            {
                consumerAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
            var consumerTypes = consumerAssemblies.SelectMany(a => a.DefinedTypes)
                .Where(t => typeof(IConsumer).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .Select(t => t.AsType())
                .ToList();
            var consumerEmptyInterface = typeof(IConsumer<>);
            foreach (var consumerType in consumerTypes)
                config.AddConsumer(consumerType);
            return config;
        }

        public static IServiceBusConfigurator AddConsumerRedelivery(this IServiceBusConfigurator config, RetryDefinition definition)
        {
            config.ThrowIfNull(nameof(config));
            config.ThrowIfNull(nameof(definition));
            config.Services.AddSingleton<IRedeliveryPipelineFactory>(new DefaultRedeliveryPipelineFactory(definition));
            return config;
        }

        public static IServiceBusConfigurator AddConsumerRetry(this IServiceBusConfigurator config, RetryDefinition definition)
        {
            config.ThrowIfNull(nameof(config));
            config.Services.AddSingleton<IRetryPipelineFactory>(new DefaultRetryPipelineFactory(definition));
            return config;
        }

        public static IServiceBusConfigurator AddConsumerTransaction(this IServiceBusConfigurator config)
        {
            config.ThrowIfNull(nameof(config));
            config.Services.AddSingleton<ITransactionPipelineFactory, DefaultTransactionPipelineFactory>();
            return config;
        }

        public static IServiceBusConfigurator AddHostedServiceBus(this IServiceBusConfigurator config)
        {
            config.ThrowIfNull(nameof(config));
            config.Services.AddHostedService<DefaultHostedService>();
            return config;
        }

        public static IServiceBusConfigurator AddConsumerTransport<T>(this IServiceBusConfigurator config) where T : class, IConsumerTransport
        {
            config.ThrowIfNull(nameof(config));
            config.Services.AddTransient<IConsumerTransport, T>();
            return config;
        }

        public static IServiceBusConfigurator AddPublisherTransport<T>(this IServiceBusConfigurator config) where T : class, IPublisherTransport
        {
            config.ThrowIfNull(nameof(config));
            config.Services.AddTransient<IPublisherTransport, T>();
            return config;
        }

        public static IServiceBusConfigurator AddConcurrencyLimiter(this IServiceBusConfigurator config, ConcurrencyDefinition definition)
        {
            config.ThrowIfNull(nameof(config));
            config.Services.AddSingleton<IConcurrencyLimiter>(new DefaultConcurrencyLimiter(definition));
            return config;
        }
    }
}
