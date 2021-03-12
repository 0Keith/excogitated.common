using Excogitated.ServiceBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class DefaultDeserializerPipeline<TConsumer, TMessage> : IConsumerPipeline
      where TConsumer : IConsumer<TMessage>
      where TMessage : class
    {
        public TConsumer Consumer { get; }
        public IServiceBusSerializer Serializer { get; }

        public DefaultDeserializerPipeline(TConsumer consumer, IServiceBusSerializer serializer)
        {
            Consumer = consumer;
            Serializer = serializer;
        }

        public async ValueTask Execute(IConsumeContext context, BinaryData messageData, ConsumerDefinition definition)
        {
            var message = Serializer.Deserialize<TMessage>(messageData);
            if (message is not null)
            {
                await Consumer.Consume(context, message);
            }
        }
    }
}
