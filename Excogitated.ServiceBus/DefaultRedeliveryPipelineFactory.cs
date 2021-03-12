using Excogitated.ServiceBus.Abstractions;

namespace Excogitated.ServiceBus
{
    internal class DefaultRedeliveryPipelineFactory : IRedeliveryPipelineFactory
    {
        public RetryDefinition Definition { get; }

        public DefaultRedeliveryPipelineFactory(RetryDefinition definition)
        {
            definition.Validate();
            Definition = definition;
        }

        public IConsumerPipeline Create(IConsumerPipeline pipeline)
        {
            return new DefaultRedeliveryPipeline(pipeline, Definition);
        }
    }
}
