using Excogitated.Mongo;
using Excogitated.ServiceBus;
using Excogitated.ServiceBus.Abstractions;
using Excogitated.ServiceBus.Mongo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Tests.NUnit
{
    [TestFixture]
    [NonParallelizable]
    public class MongoServiceBusTests
    {
        private static readonly MongoStoreSettings _settings = new()
        {
            Server = "cluster0.y9pvv.mongodb.net",
            Database = "ServiceBus",
            Username = "admin",
            Password = "WlIjAmUwms8InmDI"
        };

        private static async Task<IHost> StartPublishEndpoint()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((c, s) => s.AddDefaultServiceBus()
                .AddMongoTransport(_settings)
                .AddConcurrencyLimiter(new ConcurrencyDefinition
                {
                    PublishMaxConcurrency = 1,
                    ConsumeMaxConcurrency = 1,
                })
                .AddHostedServiceBus())
                .Build();
            await host.StartAsync();
            return host;
        }

        private static async Task<IHost> StartConsumeEndpoint()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((c, s) => s.AddDefaultServiceBus()
                    .AddHostedServiceBus()
                    .AddMongoTransport(_settings)
                    .AddConsumers()
                    .AddConsumerTransaction()
                    .AddConsumerRedelivery(new RetryDefinition { MaxDuration = TimeSpan.FromSeconds(1) })
                    .AddConsumerRetry(new RetryDefinition { MaxDuration = TimeSpan.FromSeconds(1) })
                    .AddConcurrencyLimiter(new ConcurrencyDefinition
                    {
                        PublishMaxConcurrency = 1,
                        ConsumeMaxConcurrency = 1,
                    }))
                .Build();
            await host.StartAsync();
            return host;
        }

        [SetUp]
        public async Task Setup()
        {
            using (var host = await StartConsumeEndpoint())
            {
                while (await TestObjectCreatedConsumer.WaitAsync(500)) ;
                while (await TestObjectCreatedConsumer2.WaitAsync(500)) ;
                while (await TestObjectUpdatedConsumer.WaitAsync(500)) ;
            }
            await TestObjectCreatedConsumer.ResetAsync();
            await TestObjectCreatedConsumer2.ResetAsync();
            await TestObjectUpdatedConsumer.ResetAsync();
        }

        [Test]
        public async Task Publish_Consume_Performance()
        {
            var publishedCount = 10;
            var watch = Stopwatch.StartNew();
            using (var host = await StartPublishEndpoint())
            {
                watch.Restart();
                var endpoint = host.Services.GetRequiredService<IServiceBus>();
                await Task.WhenAll(Enumerable.Range(0, publishedCount).Select(i => Task.Run(async () =>
                {
                    var msg = new TestObjectUpdated { Id = Guid.NewGuid() };
                    await endpoint.Publish(msg);
                })));
                watch.Stop();
            }
            var rate = Math.Round(publishedCount / watch.Elapsed.TotalMinutes, 2);
            var avg = Math.Round(watch.Elapsed.TotalMilliseconds / publishedCount, 2);
            var publishResult = new
            {
                Published = publishedCount,
                PublishRate = $"{rate:n} msgs per min",
                PublishAvg = $"{avg:n}ms per msg"
            };

            using (var host = await StartConsumeEndpoint())
            {
                watch.Restart();
                while (TestObjectUpdatedConsumer.Consumed < publishedCount)
                {
                    await TestObjectUpdatedConsumer.WaitAsync(1000);
                }
                watch.Stop();
            }
            rate = Math.Round(TestObjectUpdatedConsumer.Consumed / watch.Elapsed.TotalMinutes, 2);
            avg = Math.Round(watch.Elapsed.TotalMilliseconds / TestObjectUpdatedConsumer.Consumed, 2);
            var consumeResult = new
            {
                TestObjectUpdatedConsumer.Consumed,
                ConsumeRate = $"{rate:n} msgs per min",
                ConsumeAvg = $"{avg:n}ms per msg"
            };

            Console.WriteLine(publishResult);
            Console.WriteLine(consumeResult);
        }

        [Test]
        public async Task Publish_Consume()
        {
            using var host = await StartConsumeEndpoint();
            var bus = host.Services.GetRequiredService<IServiceBus>();
            var msg = new TestObjectUpdated { Id = Guid.NewGuid() };
            await bus.Publish(msg);
            await TestObjectUpdatedConsumer.WaitAsync(5000);
            Assert.AreEqual(1, TestObjectUpdatedConsumer.Consumed);
            Assert.AreEqual(0, TestObjectUpdatedConsumer.Remaining);
        }

        [Test]
        public async Task Publish_MultipleConsumers_RetryOneConsumer()
        {
            using var host = await StartConsumeEndpoint();
            var bus = host.Services.GetRequiredService<IServiceBus>();
            var msg = new TestObjectCreated { Id = Guid.NewGuid(), Retries = 1 };
            await bus.Publish(msg);
            await TestObjectCreatedConsumer.WaitAsync(5000);
            Assert.AreEqual(1, TestObjectCreatedConsumer.Consumed);
            Assert.AreEqual(0, TestObjectCreatedConsumer.Remaining);

            await TestObjectCreatedConsumer2.WaitAsync(5000);
            Assert.AreEqual(1, TestObjectCreatedConsumer2.Consumed);
            Assert.AreEqual(1, TestObjectCreatedConsumer2.Retries.Value);
            Assert.AreEqual(0, TestObjectCreatedConsumer2.Remaining);

            await TestObjectUpdatedConsumer.WaitAsync(5000); //wait for 1st message
            await TestObjectUpdatedConsumer.WaitAsync(5000); //wait for 2nd message
            Assert.AreEqual(2, TestObjectUpdatedConsumer.Consumed);
            Assert.AreEqual(0, TestObjectUpdatedConsumer.Remaining);
            Assert.NotNull(TestObjectUpdatedConsumer.LastMessage);
            Assert.AreEqual(msg.Id, TestObjectUpdatedConsumer.LastMessage.Id);
        }

        [Test, Ignore("not ready yet")]
        public async Task Publish_Consumer_Redelivery()
        {
            using var host = await StartConsumeEndpoint();
            var bus = host.Services.GetRequiredService<IServiceBus>();
            var msg = new TestObjectCreated { Id = Guid.NewGuid(), Redeliveries = 1 };
            await bus.Publish(msg);
            await TestObjectCreatedConsumer.WaitAsync(5000);
            Assert.AreEqual(1, TestObjectCreatedConsumer.Consumed);
            Assert.AreEqual(0, TestObjectCreatedConsumer.Remaining);

            await TestObjectCreatedConsumer2.WaitAsync(5000);
            Assert.AreEqual(1, TestObjectCreatedConsumer2.Consumed);
            Assert.AreEqual(1, TestObjectCreatedConsumer2.Retries.Value);
            Assert.AreEqual(0, TestObjectCreatedConsumer2.Remaining);

            await TestObjectUpdatedConsumer.WaitAsync(5000); //wait for 1st message
            await TestObjectUpdatedConsumer.WaitAsync(5000); //wait for 2nd message
            Assert.AreEqual(2, TestObjectUpdatedConsumer.Consumed);
            Assert.AreEqual(0, TestObjectUpdatedConsumer.Remaining);
            Assert.NotNull(TestObjectUpdatedConsumer.LastMessage);
            Assert.AreEqual(msg.Id, TestObjectUpdatedConsumer.LastMessage.Id);
        }
    }

}
