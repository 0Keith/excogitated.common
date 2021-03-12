using Azure.Messaging.ServiceBus;
using Excogitated.ServiceBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Azure
{
    internal class AzureConsumeContext : IConsumeContext
    {
        public ProcessMessageEventArgs Context { get; }
        public IServiceBus ServiceBus { get; }
        public ServiceBusSender Sender { get; }

        public AzureConsumeContext(ProcessMessageEventArgs context, IServiceBus serviceBus, ServiceBusSender sender)
        {
            Context = context;
            ServiceBus = serviceBus;
            Sender = sender;
        }

        public int Retries => 0;

        public int Attempts => Redeliveries + 1;

        public int Redeliveries => Context.Message.DeliveryCount;

        public DateTimeOffset InitialDeliveryDate => Context.Message.EnqueuedTime;

        public async ValueTask Reschedule(DateTimeOffset deliveryDate)
        {
            var message = new ServiceBusMessage(Context.Message);
            await Sender.ScheduleMessageAsync(message, deliveryDate);
        }

        ValueTask IConsumeContext.Publish<T>(T message)
        {
            return ServiceBus.Publish(message);
        }
    }
}
