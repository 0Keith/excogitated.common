using Azure.Messaging.ServiceBus;
using Excogitated.ServiceBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Azure
{
    internal class AzureConsumeContext : IConsumeContext
    {
        private readonly ProcessMessageEventArgs _context;
        private readonly IServiceBus _serviceBus;
        private readonly ServiceBusSender _sender;

        public AzureConsumeContext(ProcessMessageEventArgs context, IServiceBus serviceBus, ServiceBusSender sender)
        {
            _context = context;
            _serviceBus = serviceBus;
            _sender = sender;
        }

        public int Retries => 0;

        public int Attempts => Redeliveries + 1;

        public int Redeliveries => _context.Message.DeliveryCount;

        public DateTimeOffset InitialDeliveryDate => _context.Message.EnqueuedTime;

        public async ValueTask Reschedule(DateTimeOffset deliveryDate)
        {
            var message = new ServiceBusMessage(_context.Message);
            await _sender.ScheduleMessageAsync(message, deliveryDate);
        }

        ValueTask IConsumeContext.Publish<T>(T message)
        {
            return _serviceBus.Publish(message);
        }
    }
}
