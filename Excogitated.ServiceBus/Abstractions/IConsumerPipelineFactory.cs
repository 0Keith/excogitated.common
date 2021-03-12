namespace Excogitated.ServiceBus.Abstractions
{
    public interface IConsumerPipelineFactory
    {
        IConsumerPipeline Create(IConsumerPipeline pipeline);
    }
}
