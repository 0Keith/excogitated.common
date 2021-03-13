using Excogitated.ServiceBus.Abstractions;
using Excogitated.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Mongo
{
    internal class MongoPublisherTransport : IPublisherTransport, IAsyncDisposable
    {
        private readonly AtomicBool _started = new();
        private readonly AtomicBool _stopped = new();

        public ValueTask DisposeAsync() => StopAsync(default);

        public async ValueTask Publish(BinaryData message, CancellationToken cancellationToken)
        {

        }

        public async ValueTask StartAsync(PublisherDefinition definition, CancellationToken cancellationToken)
        {
            if (_started.TrySet(true))
            {

            }
        }

        public async ValueTask StopAsync(CancellationToken cancellationToken)
        {
            if (_started && _stopped.TrySet(true))
            {
            }
        }
    }
}
