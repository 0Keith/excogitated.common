using Excogitated.ServiceBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class DefaultRetryPipeline : IConsumerPipeline
    {
        public IConsumerPipeline Pipeline { get; }
        public RetryDefinition Definition { get; }

        public DefaultRetryPipeline(IConsumerPipeline pipeline, RetryDefinition definition)
        {
            Pipeline = pipeline;
            Definition = definition;
        }

        public async Task Execute(IConsumeContext context, BinaryData message, ConsumerDefinition definition)
        {
            List<Exception> exceptions = null;
            var watch = Stopwatch.StartNew();
            var interval = Definition.Interval;
            var retryContext = new DefaultRetryConsumeContext(context);
            while (true)
            {
                try
                {
                    await Pipeline.Execute(retryContext, message, definition);
                    break;
                }
                catch (Exception e)
                {
                    if (exceptions is null)
                    {
                        exceptions = new List<Exception>();
                    }
                    exceptions.Add(e);
                    if (watch.Elapsed >= Definition.MaxDuration)
                    {
                        throw new AggregateException(exceptions);
                    }
                    await Task.Delay(interval);
                    interval += Definition.Increment;
                    if (Definition.Multiplier > 0)
                    {
                        interval = TimeSpan.FromMinutes(interval.TotalMinutes * Definition.Multiplier);
                    }
                    retryContext.Increment();
                }
            }
        }
    }
}
