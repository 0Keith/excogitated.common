using Azure.Messaging.ServiceBus.Administration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.ServiceBus.Azure
{
    public class AzureTopologyDefinition
    {
        public IEnumerable<CreateQueueOptions> Queues { get; set; }
        public IEnumerable<CreateTopicOptions> Topics { get; set; }
        public IEnumerable<CreateSubscriptionOptions> Subscriptions { get; set; }
        public IEnumerable<RuleDefinition> Rules { get; set; }

        public RecieverDefinition Reciever { get; set; }
        public SenderDefinition Sender { get; set; }

        public void ValidateReciever()
        {
            if (Reciever is null)
            {
                throw new ArgumentException("Reciever must be defined in Topology to be able to recieve messages");
            }
            if (Reciever.Queue is null && Reciever.Subscription is null)
            {
                throw new ArgumentException("Reciever.Queue or Reciever.Subscription must be defined in the Reciever Topology.");
            }
            if (Reciever.Queue is object && Reciever.Subscription is object)
            {
                throw new ArgumentException("Only Reciever.Queue or Reciever.Subscription can be defined in the Reciever Topology, not both.");
            }
            if (Reciever.Queue is object && (Queues is null || !Queues.Select(q => q.Name).Contains(Reciever.Queue.Name)))
            {
                throw new ArgumentException("Reciever.Queue must also be defined in Queues to be created.");
            }
            if (Reciever.Subscription is object && (Subscriptions is null || !Subscriptions.Select(s => (s.TopicName, s.SubscriptionName))
                .Contains((Reciever.Subscription.TopicName, Reciever.Subscription.SubscriptionName))))
            {
                throw new ArgumentException("Reciever.Subscription must also be defined in Subscriptions to be created.");
            }
        }

        public void ValidateSender()
        {
            if (Sender is null)
            {
                throw new ArgumentException("Sender must be defined in Topology to be able to send messages");
            }
            if (Sender.Queue is null && Sender.Topic is null)
            {
                throw new ArgumentException("Sender.Queue or Sender.Topic must be defined in the Sender Topology.");
            }
            if (Sender.Queue is object && Sender.Topic is object)
            {
                throw new ArgumentException("Only Sender.Queue or Sender.Topic can be defined in the Sender Topology, not both.");
            }
            if (Sender.Queue is object && (Queues is null || !Queues.Select(q => q.Name).Contains(Sender.Queue.Name)))
            {
                throw new ArgumentException("Sender.Queue must also be defined in Queues to be created.");
            }
            if (Sender.Topic is object && (Topics is null || !Topics.Select(t => t.Name).Contains(Sender.Topic.Name)))
            {
                throw new ArgumentException("Sender.Topic must also be defined in Topics to be created.");
            }
        }
    }

    public class RecieverDefinition
    {
        public CreateQueueOptions Queue { get; set; }
        public CreateSubscriptionOptions Subscription { get; set; }
    }

    public class SenderDefinition
    {
        public CreateQueueOptions Queue { get; set; }
        public CreateTopicOptions Topic { get; set; }
    }

    public class RuleDefinition
    {
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
        public CreateRuleOptions Options { get; internal set; }
    }
}
