using Excogitated.ServiceBus.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class TransactionConsumeContext : IConsumeContext
    {
        private readonly ConcurrentQueue<Func<Task>> _messages = new ConcurrentQueue<Func<Task>>();
        private readonly IConsumeContext _context;

        public TransactionConsumeContext(IConsumeContext context)
        {
            _context = context;
        }

        public int Retries => _context.Retries;

        public int Attempts => _context.Attempts;

        public int Redeliveries => _context.Redeliveries;

        public DateTimeOffset InitialDeliveryDate => _context.InitialDeliveryDate;

        public Task Publish<T>(T message) where T : class
        {
            _messages.Enqueue(() => _context.Publish(message));
            return Task.CompletedTask;
        }

        public Task Reschedule(DateTimeOffset rescheduleDate)
        {
            return _context.Reschedule(rescheduleDate);
        }

        internal async Task PublishMessages()
        {
            while (_messages.TryDequeue(out var publishMessage))
            {
                await publishMessage();
            }
        }
    }
}
