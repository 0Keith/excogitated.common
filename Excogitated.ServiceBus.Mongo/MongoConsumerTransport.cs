using Excogitated.ServiceBus.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Mongo
{
    internal class MongoConsumerTransport : IConsumerTransport
    {
        public ValueTask StartAsync(ConsumerDefinition consumerDefinition, IConsumerPipeline pipeline, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}