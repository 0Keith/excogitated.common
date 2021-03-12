﻿using Excogitated.ServiceBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class DefaultConsumerPipeline : IConsumerPipeline
    {
        private readonly IServiceProvider _provider;

        public DefaultConsumerPipeline(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task Execute(IConsumeContext context, BinaryData message, ConsumerDefinition definition)
        {
            using var scope = _provider.CreateScope();
            var pipeline = scope.ServiceProvider.GetRequiredService(definition.PipelineType) as IConsumerPipeline
                ?? throw new ArgumentException($"{definition.PipelineType} does not implement {typeof(IConsumerPipeline).FullName}");
            var factories = new IConsumerPipelineFactory[] {
                scope.ServiceProvider.GetService<ITransactionPipelineFactory>(),
                scope.ServiceProvider.GetService<IRetryPipelineFactory>(),
                scope.ServiceProvider.GetService<IRedeliveryPipelineFactory>()
            };
            foreach (var f in factories)
            {
                if (f is not null)
                {
                    pipeline = f.Create(pipeline);
                }
            }
            await pipeline.Execute(context, message, definition);
        }
    }
}
