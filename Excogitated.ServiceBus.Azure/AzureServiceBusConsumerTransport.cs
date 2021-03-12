using Azure.Messaging.ServiceBus;
using Excogitated.Common.Atomic;
using Excogitated.ServiceBus.Abstractions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Azure
{
    internal class AzureServiceBusConsumerTransport : IConsumerTransport, IAsyncDisposable
    {
        private readonly AtomicInt32 _totalMessagesProcessed = new AtomicInt32();
        private readonly AtomicInt64 _totalProcessTime = new AtomicInt64();

        private readonly AzureClientFactory _clientFactory;
        private readonly IAzureTopologyBuilder _topologyBuilder;
        private readonly IServiceBus _serviceBus;
        private ServiceBusProcessor _processor;
        private ServiceBusSender _sender;
        private ConsumerDefinition _consumerDefinition;

        public AzureServiceBusConsumerTransport(AzureClientFactory clientFactory, IAzureTopologyBuilder topologyBuilder, IServiceBus serviceBus)
        {
            _clientFactory = clientFactory;
            _topologyBuilder = topologyBuilder;
            _serviceBus = serviceBus;
        }

        public ValueTask DisposeAsync() => new ValueTask(StopAsync());

        public async Task StartAsync(ConsumerDefinition consumerDefinition, IConsumerPipeline pipeline, CancellationToken cancellationToken)
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

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            Stop(cancellationToken);
            return Task.CompletedTask;
        }

        //ServiceBusProcessor.StopProcessingAsync currently has a bug when using the AMQP protocol that prevents it from gracefully shutting down in a timely manner
        //it will timeout after 1 min and eventually shutdown but we don't want to wait for that so do all shutdown waiting in an async void method.
        private async void Stop(CancellationToken cancellationToken)
        {
            var processor = _processor;
            if (processor is not null && !processor.IsClosed)
            {
                Console.WriteLine(new
                {
                    TotalMessagesProcessed = _totalMessagesProcessed.Value,
                    TotalProcessTime = TimeSpan.FromTicks(_totalProcessTime.Value),
                    AvgTimePerMsg = _totalMessagesProcessed.Value > 0 ? TimeSpan.FromTicks(_totalProcessTime.Value / _totalMessagesProcessed.Value) : TimeSpan.Zero,
                    Consumer = _consumerDefinition.ConsumerType.FullName
                });
                await using (processor)
                {
                    await _processor.StopProcessingAsync(cancellationToken);
                    await _processor.CloseAsync(cancellationToken);
                }
            }
        }
    }
}
