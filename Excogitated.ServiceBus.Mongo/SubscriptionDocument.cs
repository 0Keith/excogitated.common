using System;

namespace Excogitated.ServiceBus.Mongo
{
    internal class SubscriptionDocument
    {
        public Guid Id { get; set; }
        public string TopicName { get; set; }
        public string QueueName { get; set; }
    }
}