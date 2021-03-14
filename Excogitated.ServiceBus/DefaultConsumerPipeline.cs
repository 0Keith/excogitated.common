using Excogitated.ServiceBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class DefaultConsumerPipeline : IConsumerPipeline
    {
        public IServiceProvider Provider { get; }
        public IConcurrencyLimiter ConcurrencyLimiter { get; }

        public DefaultConsumerPipeline(IServiceProvider provider, IConcurrencyLimiter concurrencyLimiter)
        {
            Provider = provider;
            ConcurrencyLimiter = concurrencyLimiter;
        }

        public async ValueTask Execute(IConsumeContext context, BinaryData message, ConsumerDefinition definition)
        {
            using var scope = Provider.CreateScope();
            var pipeline = scope.ServiceProvider.GetRequiredService(definition.PipelineType) as IConsumerPipeline
                ?? throw new ArgumentException($"{definition.PipelineType} does not implement {typeof(IConsumerPipeline).FullName}");
            var factories = new IConsumerPipelineFactory[] {
                scope.ServiceProvider.GetService<ITransactionPipelineFactory>(),
                scope.ServiceProvider.GetService<IRetryPipelineFactory>(),
                scope.ServiceProvider.GetService<IRedeliveryPipelineFactory>()
            };
            foreach (var factory in factories)
            {
                if (factory is not null)
                {
                    pipeline = factory.Create(pipeline);
                }
            }
            using (await ConcurrencyLimiter.AcquireConsumerSlot())
                await pipeline.Execute(context, message, definition);
        }
    }
}
