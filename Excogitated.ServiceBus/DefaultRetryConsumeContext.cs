using Excogitated.Common.Atomic;
using Excogitated.ServiceBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class DefaultRetryConsumeContext : IConsumeContext
    {
        private readonly AtomicInt32 _retries = new();

        public IConsumeContext Context { get; }

        public DefaultRetryConsumeContext(IConsumeContext context)
        {
            Context = context;
        }

        public int Retries => Context.Retries + _retries;

        public int Attempts => Context.Attempts + _retries + 1;

        public int Redeliveries => Context.Redeliveries;

        public DateTimeOffset InitialDeliveryDate => Context.InitialDeliveryDate;

        public void Increment() => _retries.Increment();

        public ValueTask Publish<T>(T message) where T : class
        {
            return Context.Publish(message);
        }

        public ValueTask Reschedule(DateTimeOffset deliveryDate)
        {
            return Context.Reschedule(deliveryDate);
        }
    }
}