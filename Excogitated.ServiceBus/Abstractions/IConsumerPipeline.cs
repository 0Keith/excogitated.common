using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IConsumerPipeline
    {
        Task Execute(IConsumeContext context, BinaryData message, ConsumerDefinition definition);
    }
}
