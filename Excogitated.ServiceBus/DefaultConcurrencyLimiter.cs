using Excogitated.ServiceBus.Abstractions;
using Excogitated.Threading;
using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus
{
    internal class DefaultConcurrencyLimiter : IConcurrencyLimiter
    {
        private readonly AsyncLimiter _publishLimiter;
        private readonly AsyncLimiter _consumeLimiter;

        public ConcurrencyDefinition Definition { get; }

        public ValueTask<IDisposable> AcquirePublishSlot() => _publishLimiter.WaitAsync();
        public ValueTask<IDisposable> AcquireConsumerSlot() => _consumeLimiter.WaitAsync();

        public DefaultConcurrencyLimiter(ConcurrencyDefinition definition)
        {
            Definition = definition;
            _publishLimiter = new AsyncLimiter(definition.PublishMaxConcurrency, definition.PublishMaxRate, definition.PublishRateInterval);
            _consumeLimiter = new AsyncLimiter(definition.ConsumeMaxConcurrency, definition.ConsumeMaxRate, definition.ConsumeRateInterval);
        }
    }
}