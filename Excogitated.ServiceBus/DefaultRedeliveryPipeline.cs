using Excogitated.ServiceBus.Abstractions;
using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class DefaultRedeliveryPipeline : IConsumerPipeline
    {
        public IConsumerPipeline Pipeline { get; }
        public RetryDefinition Definition { get; }

        public DefaultRedeliveryPipeline(IConsumerPipeline pipeline, RetryDefinition definition)
        {
            Pipeline = pipeline;
            Definition = definition;
        }

        public async Task Execute(IConsumeContext context, BinaryData message, ConsumerDefinition definition)
        {
            try
            {
                await Pipeline.Execute(context, message, definition);
            }
            catch (Exception)
            {
                var elapsed = DateTimeOffset.Now - context.InitialDeliveryDate;
                if (elapsed < Definition.MaxDuration)
                {
                    var interval = Definition.Interval;
                    var redeliveryDate = context.InitialDeliveryDate + interval;
                    while (redeliveryDate <= DateTimeOffset.Now)
                    {
                        interval += Definition.Increment;
                        if (Definition.Multiplier > 0)
                        {
                            interval = TimeSpan.FromMinutes(interval.TotalMinutes * Definition.Multiplier);
                        }
                        redeliveryDate += interval;
                    }
                    await context.Reschedule(redeliveryDate);
                }
                throw;
            }
        }
    }
}
