using Azure.Messaging.ServiceBus;
using Excogitated.Common.Atomic;
using Excogitated.ServiceBus.Abstractions;
using Excogitated.ServiceBus.Azure.Abstractions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Azure
{
    internal class AzureConsumerTransport : IConsumerTransport, IAsyncDisposable
    {
        private readonly AtomicInt32 _totalMessagesProcessed = new();
        private readonly AtomicInt64 _totalProcessTime = new();
        private readonly AtomicBool _started = new();
        private readonly AtomicBool _stopped = new();

        private readonly IAzureClientFactory _clientFactory;
        private readonly IAzureTopologyBuilder _topologyBuilder;
        private readonly IServiceBus _serviceBus;
        private ServiceBusProcessor _processor;
        private ServiceBusSender _sender;
        private ConsumerDefinition _consumerDefinition;

        public AzureConsumerTransport(IAzureClientFactory clientFactory, IAzureTopologyBuilder topologyBuilder, IServiceBus serviceBus)
        {
            _clientFactory = clientFactory;
            _topologyBuilder = topologyBuilder;
            _serviceBus = serviceBus;
        }

        public ValueTask DisposeAsync() => StopAsync(default);

        public async ValueTask StartAsync(ConsumerDefinition consumerDefinition, IConsumerPipeline pipeline, CancellationToken cancellationToken)
        {
            if (_started.TrySet(true))
            {
                _consumerDefinition = consumerDefinition;
                var topologyDefinition = _topologyBuilder.BuildFrom(consumerDefinition);
                _processor = await _clientFactory.BuildProcessorAsync(topologyDefinition, cancellationToken);
                _sender = await _clientFactory.BuildSenderAsync(topologyDefinition, cancellationToken);
                _processor.ProcessMessageAsync += async e =>
                {
                    _totalMessagesProcessed.Increment();
                    var watch = Stopwatch.StartNew();
                    var context = new AzureConsumeContext(e, _serviceBus, _sender);
                    await pipeline.Execute(context, e.Message.Body, consumerDefinition);
                    _totalProcessTime.Add(watch.ElapsedTicks);
                };
                _processor.ProcessErrorAsync += async e =>
                {
                    Console.WriteLine(e.Exception);
                };
                await _processor.StartProcessingAsync(cancellationToken);
            }
        }

        public ValueTask StopAsync(CancellationToken cancellationToken)
        {
            Stop(cancellationToken);
            return new();
        }

        //ServiceBusProcessor.StopProcessingAsync currently has a bug when using the AMQP protocol that prevents it from gracefully shutting down in a timely manner
        //it will timeout after 1 min and eventually shutdown but we don't want to wait for that so do all shutdown waiting in an async void method.
        private async void Stop(CancellationToken cancellationToken)
        {
            if (_started && _stopped.TrySet(true))
            {
                Console.WriteLine(new
                {
                    TotalMessagesProcessed = _totalMessagesProcessed.Value,
                    TotalProcessTime = TimeSpan.FromTicks(_totalProcessTime.Value),
                    AvgTimePerMsg = _totalMessagesProcessed.Value > 0 ? TimeSpan.FromTicks(_totalProcessTime.Value / _totalMessagesProcessed.Value) : TimeSpan.Zero,
                    Consumer = _consumerDefinition.ConsumerType.FullName
                });
                await using (_sender)
                await using (_processor)
                {
                    await _processor.StopProcessingAsync(cancellationToken);
                    await _processor.CloseAsync(cancellationToken);
                    await _sender.CloseAsync(cancellationToken);
                }
            }
        }
    }
}
