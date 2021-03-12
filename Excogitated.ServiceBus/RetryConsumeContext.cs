using Excogitated.Common.Atomic;
using Excogitated.ServiceBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class RetryConsumeContext : IConsumeContext
    {
        private readonly AtomicInt32 _retries = new AtomicInt32();

        public IConsumeContext Context { get; }

        public int Retries => Context.Retries + _retries;

        public int Attempts => Context.Attempts + _retries + 1;

        public int Redeliveries => Context.Redeliveries;

        public DateTimeOffset InitialDeliveryDate => Context.InitialDeliveryDate;

        public RetryConsumeContext(IConsumeContext context)
        {
            Context = context;
        }

        public Task Publish<T>(T message) where T : class
        {
            return Context.Publish(message);
        }

        public Task Reschedule(DateTimeOffset deliveryDate)
        {
            return Context.Reschedule(deliveryDate);
        }

        internal void Increment() => _retries.Increment();
    }
}
