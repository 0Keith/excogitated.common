using Azure.Messaging.ServiceBus.Administration;
using Excogitated.ServiceBus.Abstractions;
using System;

namespace Excogitated.ServiceBus.Azure
{
    internal class AzureForwardingTopologyBuilder : IAzureTopologyBuilder
    {
        private readonly string _entityPrefix;

        public AzureForwardingTopologyBuilder(string entityPrefix)
        {
            _entityPrefix = entityPrefix;
        }

        public AzureTopologyDefinition BuildFrom(PublisherDefinition definition)
        {
            var topic = BuildTopic(definition.MessageType);
            return new AzureTopologyDefinition
            {
                Topics = new[] { topic },
                Sender = new SenderDefinition { Topic = topic }
            };
        }

        public AzureTopologyDefinition BuildFrom(ConsumerDefinition definition)
        {
            var queue = BuildQueue(definition.ConsumerType);
            var topic = BuildTopic(definition.MessageType);
            var subscription = BuildSubscription(topic.Name, queue.Name, definition.ConsumerType);
            return new AzureTopologyDefinition
            {
                Queues = new[] { queue },
                Topics = new[] { topic },
                Subscriptions = new[] { subscription },
                Reciever = new RecieverDefinition { Queue = queue },
                Sender = new SenderDefinition { Queue = queue }
            };
        }

        private CreateQueueOptions BuildQueue(Type consumerType)
        {
            var path = consumerType.FullName
                .Shorten(260, _entityPrefix) //max queue name length is 260 chars
                .ToLower();
            return new CreateQueueOptions(path)
            {
                LockDuration = TimeSpan.FromMinutes(5)
            };
        }

        private CreateTopicOptions BuildTopic(Type messageType)
        {
            var path = messageType.FullName
                .Shorten(260, _entityPrefix) //max topic name length is 260 chars
                .ToLower();
            return new CreateTopicOptions(path)
            {
            };
        }

        private CreateSubscriptionOptions BuildSubscription(string topicName, string queueName, Type consumerType)
        {
            var name = consumerType.FullName
                .Shorten(50, _entityPrefix) //max subscription name length is 50 chars
                .ToLower();
            return new CreateSubscriptionOptions(topicName, name)
            {
                ForwardTo = queueName
            };
        }
    }
}
