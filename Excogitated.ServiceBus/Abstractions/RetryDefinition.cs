using System;

namespace Excogitated.ServiceBus.Abstractions
{
    public struct RetryDefinition
    {
        public TimeSpan MaxDuration { get; set; }
        public TimeSpan Interval { get; set; }
        public TimeSpan Increment { get; set; }
        public int Multiplier { get; set; }

        public void Validate()
        {
            if (Interval < TimeSpan.Zero)
                throw new ArgumentException("Interval must be greater than or equal to zero");
            if (Increment < TimeSpan.Zero)
                throw new ArgumentException("Increment must be greater than or equal to zero");
            if (Multiplier < 0)
                throw new ArgumentException("Multiplier must be greater than or equal to zero");
        }
    }
}
