using Excogitated.ServiceBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class DefaultDeserializerPipeline<TConsumer, TMessage> : IConsumerPipeline
      where TConsumer : IConsumer<TMessage>
      where TMessage : class
    {
        private readonly TConsumer _consumer;
        private readonly IServiceBusSerializer _serializer;

        public DefaultDeserializerPipeline(TConsumer consumer, IServiceBusSerializer serializer)
        {
            _consumer = consumer;
            _serializer = serializer;
        }

        public async Task Execute(IConsumeContext context, BinaryData messageData, ConsumerDefinition definition)
        {
            var message = _serializer.Deserialize<TMessage>(messageData);
            if (message is not null)
            {
                await _consumer.Consume(context, message);
            }
        }
    }
}
