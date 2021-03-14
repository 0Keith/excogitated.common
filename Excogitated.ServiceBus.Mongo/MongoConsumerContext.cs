using Excogitated.ServiceBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Mongo
{
    internal class MongoConsumerContext : IConsumeContext
    {
        public MongoConsumerContext()
        {
        }

        public int Retries { get; }
        public int Attempts { get; }
        public int Redeliveries { get; }
        public DateTimeOffset InitialDeliveryDate { get; }

        public ValueTask Publish<T>(T message) where T : class
        {
            throw new NotImplementedException();
        }

        public ValueTask Reschedule(DateTimeOffset deliveryDate)
        {
            throw new NotImplementedException();
        }
    }
}