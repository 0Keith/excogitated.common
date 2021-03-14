using System;

namespace Excogitated.ServiceBus
{
    public struct ConcurrencyDefinition
    {
        public int PublishMaxConcurrency { get; set; }
        public int PublishMaxRate { get; set; }
        public TimeSpan PublishRateInterval { get; set; }
        public int ConsumeMaxConcurrency { get; set; }
        public int ConsumeMaxRate { get; set; }
        public TimeSpan ConsumeRateInterval { get; set; }
    }
}