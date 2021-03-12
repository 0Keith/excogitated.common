using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Excogitated.ServiceBus.Azure.Abstractions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Azure
{
    internal class AzureClientFactory : IAzureClientFactory, IAsyncDisposable
    {
        private readonly ServiceBusAdministrationClient _admin;
        private readonly ServiceBusClient _client;
        private readonly TransportSettings _settings;

        public async ValueTask DisposeAsync() => await _client.ConfigureAwait(false).DisposeAsync();

        public AzureClientFactory(TransportSettings settings)
        {
            _admin = new ServiceBusAdministrationClient(settings.ConnectionString);
            _client = new ServiceBusClient(settings.ConnectionString);
            _settings = settings;
        }

        public async Task<ServiceBusProcessor> BuildProcessorAsync(AzureTopologyDefinition topologyDefinition, CancellationToken cancellationToken)
        {
            //validate topology before creating
            topologyDefinition.ValidateReciever();
            await BuildEntitiesAsync(topologyDefinition, cancellationToken);

            //create options for processor
            var options = new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = true,
                MaxAutoLockRenewalDuration = TimeSpan.FromHours(_settings.MaxAutoRenewHours),
                ReceiveMode = ServiceBusReceiveMode.PeekLock,

                //set both to 1 when debugging to make debugging easier
                //we set both the MaxConcurrentCalls and the PrefetchCount together because their behaviors are intrinsically linked
                //setting PrefetchCount greater than MaxConcurrentCalls can cause message locks to expire before they are processed and overall slowdown processing
                //setting MaxConcurrentCalls greater than PrefetchCount offers almost zero performance benefits
                MaxConcurrentCalls = Debugger.IsAttached ? 1 : _settings.PrefetchCount,
                PrefetchCount = Debugger.IsAttached ? 1 : _settings.PrefetchCount
            };

            //create queue reciever
            if (topologyDefinition.Reciever.Queue is CreateQueueOptions queue)
            {
                return _client.CreateProcessor(queue.Name, options);
            }

            //create subscription reciever
            if (topologyDefinition.Reciever.Subscription is CreateSubscriptionOptions subscription)
            {
                return _client.CreateProcessor(subscription.TopicName, subscription.SubscriptionName, options);
            }

            //this should be caught by topology validation
            throw new ArgumentException("No reciever was configured in topology. This should never happen.");
        }

        public async Task<ServiceBusSender> BuildSenderAsync(AzureTopologyDefinition topologyDefinition, CancellationToken cancellationToken = default)
        {
            //validate topology before creating
            topologyDefinition.ValidateSender();
            await BuildEntitiesAsync(topologyDefinition);

            //create queue sender
            if (topologyDefinition.Sender.Queue is CreateQueueOptions queue)
            {
                return _client.CreateSender(queue.Name);
            }

            //create topic sender
            if (topologyDefinition.Sender.Topic is CreateTopicOptions topic)
            {
                return _client.CreateSender(topic.Name);
            }

            //this should be caught by topology validation
            throw new ArgumentException("No sender was configured in topology. This should never happen.");
        }

        private async Task BuildEntitiesAsync(AzureTopologyDefinition topologyDefinition, CancellationToken cancellationToken = default)
        {
            //create any defined queues
            if (topologyDefinition.Queues is object)
            {
                foreach (var queue in topologyDefinition.Queues)
                {
                    if (!await _admin.QueueExistsAsync(queue.Name, cancellationToken))
                    {
                        await _admin.CreateQueueAsync(queue, cancellationToken);
                    }
                }
            }

            //create any defined topics
            if (topologyDefinition.Topics is object)
            {
                foreach (var topic in topologyDefinition.Topics)
                {
                    if (!await _admin.TopicExistsAsync(topic.Name, cancellationToken))
                    {
                        await _admin.CreateTopicAsync(topic, cancellationToken);
                    }
                }
            }

            //create any defined subscriptions
            if (topologyDefinition.Subscriptions is object)
            {
                foreach (var subscription in topologyDefinition.Subscriptions)
                {
                    if (!await _admin.SubscriptionExistsAsync(subscription.TopicName, subscription.SubscriptionName, cancellationToken))
                    {
                        await _admin.CreateSubscriptionAsync(subscription, cancellationToken);
                    }
                }
            }

            //create any defined rules
            if (topologyDefinition.Rules is object)
            {
                foreach (var rule in topologyDefinition.Rules)
                {
                    if (!await _admin.RuleExistsAsync(rule.TopicName, rule.SubscriptionName, rule.Options.Name, cancellationToken))
                    {
                        await _admin.CreateRuleAsync(rule.TopicName, rule.SubscriptionName, rule.Options, cancellationToken);
                    }
                }
            }
        }
    }
}
