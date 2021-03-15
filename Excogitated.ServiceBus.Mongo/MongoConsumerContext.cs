using Excogitated.ServiceBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Mongo
{
    internal class MongoConsumerContext : IConsumeContext
    {
        public int Retries { get; }
        public int Attempts { get; }
        public int Redeliveries { get; }
        public DateTimeOffset InitialDeliveryDate { get; }
        public IServiceBus ServiceBus { get; }

        public MongoConsumerContext(IServiceBus serviceBus)
        {
            ServiceBus = serviceBus;
        }

        public ValueTask Publish<T>(T message) where T : class
        {
            return ServiceBus.Publish(message);
        }

        public ValueTask Reschedule(DateTimeOffset deliveryDate)
        {
            throw new NotImplementedException();
        }
    }
}