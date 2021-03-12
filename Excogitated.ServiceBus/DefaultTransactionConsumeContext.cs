using Excogitated.ServiceBus.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class DefaultTransactionConsumeContext : IConsumeContext
    {
        private readonly ConcurrentQueue<Func<ValueTask>> _messages = new();

        public IConsumeContext Context { get; }

        public DefaultTransactionConsumeContext(IConsumeContext context)
        {
            Context = context;
        }

        public int Retries => Context.Retries;

        public int Attempts => Context.Attempts;

        public int Redeliveries => Context.Redeliveries;

        public DateTimeOffset InitialDeliveryDate => Context.InitialDeliveryDate;

        public ValueTask Publish<T>(T message) where T : class
        {
            _messages.Enqueue(() => Context.Publish(message));
            return new();
        }

        public ValueTask Reschedule(DateTimeOffset rescheduleDate)
        {
            return Context.Reschedule(rescheduleDate);
        }

        public async ValueTask PublishMessages()
        {
            while (_messages.TryDequeue(out var publishMessage))
            {
                await publishMessage();
            }
        }
    }
}
