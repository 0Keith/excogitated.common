using System;

namespace Excogitated.ServiceBus.Abstractions
{
    public class ConsumerDefinition
    {
        public Type ConsumerType { get; }
        public Type ConsumerInterface { get; }
        public Type MessageType { get; }
        public Type PipelineType { get; }

        public ConsumerDefinition(Type consumerType, Type consumerInterface, Type messageType, Type pipelineType)
        {
            ConsumerType = consumerType ?? throw new ArgumentNullException(nameof(consumerType));
            ConsumerInterface = consumerInterface ?? throw new ArgumentNullException(nameof(consumerInterface));
            MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
            PipelineType = pipelineType ?? throw new ArgumentNullException(nameof(pipelineType));
        }
    }
}
