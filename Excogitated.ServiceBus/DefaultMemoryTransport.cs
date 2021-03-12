using Excogitated.Common.Atomic;
using Excogitated.ServiceBus.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class DefaultMemoryTransport : IConsumerTransport, IPublisherTransport, IAsyncDisposable
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, Channel<BinaryData>>> _memoryBus = new();

        private readonly CancellationTokenSource _running = new();
        private readonly AtomicBool _stopped = new();

        private ConcurrentDictionary<Type, Channel<BinaryData>> _consumers;
        private Channel<BinaryData> _channel;

        public IServiceBus ServiceBus { get; }

        public ValueTask DisposeAsync() => StopAsync(default);

        public DefaultMemoryTransport(IServiceBus serviceBus)
        {
            ServiceBus = serviceBus;
        }

        public async ValueTask Publish(BinaryData message, CancellationToken cancellationToken)
        {
            var tasks = _consumers.Values
                .Where(c => !c.Reader.Completion.IsCompleted)
                .Select(c => c.Writer.WriteAsync(message, cancellationToken).AsTask());
            await Task.WhenAll(tasks);
        }

        public ValueTask StartAsync(ConsumerDefinition definition, IConsumerPipeline pipeline, CancellationToken cancellationToken)
        {
            _channel = _memoryBus.GetOrAdd(definition.MessageType, t => new())
                .GetOrAdd(definition.ConsumerType, t => Channel.CreateUnbounded<BinaryData>());
            var consumer = _channel.Reader;
            var sender = _channel.Writer;
            //need way to configure concurrency
            Enumerable.Range(0, 100).Select(i => Task.Run(async () =>
            {
                while (!_running.IsCancellationRequested)
                {
                    var message = await consumer.ReadAsync(_running.Token);
                    if (_stopped)
                        await sender.WriteAsync(message);
                    else
                        try
                        {
                            if (!_running.IsCancellationRequested)
                            {
                                var context = new DefaultMemoryConsumeContext(message, ServiceBus, sender);
                                await pipeline.Execute(context, message, definition);
                            }
                        }
                        catch (Exception e)
                        {
                            await sender.WriteAsync(message);
                            Console.WriteLine(e); //really need better logging
                        }
                }
            })).ToList();
            return new();
        }

        public ValueTask StartAsync(PublisherDefinition definition, CancellationToken cancellationToken)
        {
            _consumers = _memoryBus.GetOrAdd(definition.MessageType, m => new());
            return new();
        }

        public async ValueTask StopAsync(CancellationToken cancellationToken)
        {
            if (_stopped.TrySet(true))
            {
                _running.Cancel();
            }
        }
    }
}