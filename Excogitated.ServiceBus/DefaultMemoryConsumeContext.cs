using Excogitated.ServiceBus.Abstractions;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class DefaultMemoryConsumeContext : IConsumeContext
    {
        public BinaryData Message { get; }
        public IServiceBus ServiceBus { get; }
        public ChannelWriter<BinaryData> Sender { get; }

        public DefaultMemoryConsumeContext(BinaryData message, IServiceBus serviceBus, ChannelWriter<BinaryData> sender)
        {
            Message = message;
            ServiceBus = serviceBus;
            Sender = sender;
        }

        public int Retries { get; }
        public int Attempts { get; }
        public int Redeliveries { get; }
        public DateTimeOffset InitialDeliveryDate { get; }

        public ValueTask Publish<T>(T message) where T : class
        {
            return ServiceBus.Publish(message);
        }

        public ValueTask Reschedule(DateTimeOffset deliveryDate)
        {
            RescheduleAsync(deliveryDate);
            return new();
        }

        public async void RescheduleAsync(DateTimeOffset deliveryDate)
        {
            var timeRemaining = deliveryDate - DateTimeOffset.Now;
            while (timeRemaining.TotalSeconds > 60)
            {
                await Task.Delay(TimeSpan.FromSeconds(60));
                timeRemaining = deliveryDate - DateTimeOffset.Now;
            }
            if (timeRemaining.TotalSeconds > 0)
                await Task.Delay(timeRemaining);
            await Sender.WriteAsync(Message);
        }
    }
}