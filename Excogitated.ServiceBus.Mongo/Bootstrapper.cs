using Excogitated.Extensions;
using Excogitated.Mongo;
using Excogitated.ServiceBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Excogitated.ServiceBus.Mongo
{
    public static class Bootstrapper
    {
        public static IServiceBusConfigurator AddMongoTransport(this IServiceBusConfigurator config, MongoStoreSettings settings)
        {
            config.ThrowIfNull(nameof(config))
                .AddPublisherTransport<MongoPublisherTransport>()
                .AddConsumerTransport<MongoConsumerTransport>();
            config.Services.AddSingleton(settings.GetDatabase());
            return config;
        }
    }
}
