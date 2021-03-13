using Excogitated.Extensions;
using Excogitated.Mongo;
using Excogitated.ServiceBus.Abstractions;

namespace Excogitated.ServiceBus.Mongo
{
    public static class Bootstrapper
    {
        public static IServiceBusConfigurator AddMongoTransport(this IServiceBusConfigurator config, MongoStoreSettings settings)
        {
            return config.ThrowIfNull(nameof(config))
                .AddPublisherTransport<MongoPublisherTransport>()
                .AddConsumerTransport<MongoConsumerTransport>();
        }
    }
}
