using Excogitated.ServiceBus.Abstractions;

namespace Excogitated.ServiceBus
{
    internal class DefaultTransactionPipelineFactory : ITransactionPipelineFactory
    {
        public IConsumerPipeline Create(IConsumerPipeline pipeline)
        {
            return new DefaultTransactionPipeline(pipeline);
        }
    }
}
