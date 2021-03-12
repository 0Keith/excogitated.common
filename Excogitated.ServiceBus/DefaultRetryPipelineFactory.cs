using Excogitated.ServiceBus.Abstractions;

namespace Excogitated.ServiceBus
{
    internal class DefaultRetryPipelineFactory : IRetryPipelineFactory
    {
        public RetryDefinition Definition { get; }

        public DefaultRetryPipelineFactory(RetryDefinition definition)
        {
            definition.Validate();
            Definition = definition;
        }

        public IConsumerPipeline Create(IConsumerPipeline pipeline)
        {
            return new DefaultRetryPipeline(pipeline, Definition);
        }
    }
}
