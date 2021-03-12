using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IConsumerPipeline
    {
        ValueTask Execute(IConsumeContext context, BinaryData message, ConsumerDefinition definition);
    }
}
