using Excogitated.Common.Atomic;
using Excogitated.ServiceBus;
using Excogitated.ServiceBus.Abstractions;
using Excogitated.ServiceBus.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Excogitated.Tests.NUnit
{
    [TestFixture]
    [NonParallelizable]
    public class ServiceBusTests
    {
        private static async Task<IHost> StartPublishEndpoint()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((c, s) => s.StartHostedServiceBusWithAzureTransport())
                .Build();
            await host.StartAsync();
            return host;
        }

        private static async Task<IHost> StartConsumeEndpoint()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((c, s) => s.StartHostedServiceBusWithAzureTransport(typeof(ServiceBusTests).Assembly)
                .AddServiceBusRedelivery(new RetryDefinition { MaxDuration = TimeSpan.FromSeconds(1) })
                .AddServiceBusRetry(new RetryDefinition { MaxDuration = TimeSpan.FromSeconds(1) }))
                .Build();
            await host.StartAsync();
            return host;
        }

        private static async Task<IHost> StartMemoryEndpoint()
        {
            var host = Host.CreateDefaultBuilder()
                //.ConfigureServices((c, s) => s.StartMassTransitHostedServiceWithMemoryTransport(c.Configuration, typeof(EndpointTests).Assembly))
                .Build();
            await host.StartAsync();
            return host;
        }

        [SetUp]
        public async Task Setup()
        {
            using (var host = await StartConsumeEndpoint())
            {
                await Task.Delay(5000); //let queues clear
            }
            await TestObjectCreatedConsumer.ResetAsync();
            await TestObjectCreatedConsumer2.ResetAsync();
            await TestObjectUpdatedConsumer.ResetAsync();
        }

        [Test]
        public async Task Publish_Consume_Performance()
        {
            var publishedCount = 1000;
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
                PublishRate = $"{rate} msgs per min",
                PublishAvg = $"{avg}ms per msg"
            };

            using (var host = await StartConsumeEndpoint())
            {
                watch.Restart();
                while (TestObjectUpdatedConsumer.Consumed < publishedCount)
                {
                    await TestObjectUpdatedConsumer.WaitAsync(5000);
                }
                watch.Stop();
            }
            rate = Math.Round(TestObjectUpdatedConsumer.Consumed / watch.Elapsed.TotalMinutes, 2);
            avg = Math.Round(watch.Elapsed.TotalMilliseconds / TestObjectUpdatedConsumer.Consumed, 2);
            var consumeResult = new
            {
                ConsumeRate = $"{rate} msgs per min",
                ConsumeAvg = $"{avg}ms per msg"
            };

            Console.WriteLine(publishResult);
            Console.WriteLine(consumeResult);
        }

        [Test]
        public async Task Publish_Consume()
        {
            using var host = await StartConsumeEndpoint();
            var bus = host.Services.GetRequiredService<IServiceBus>();
            var msg = new TestObjectCreated { Id = Guid.NewGuid() };
            await bus.Publish(msg);
            await TestObjectCreatedConsumer.WaitAsync(5000);
            Assert.AreEqual(1, TestObjectCreatedConsumer.Consumed);
            Assert.AreEqual(0, TestObjectCreatedConsumer.Remaining);
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

    public abstract class TestObjectConsumerBase<TConsumer, TMessage>
    {
        protected static readonly SemaphoreSlim _sync = new SemaphoreSlim(0);
        protected static readonly AtomicInt32 _consumed = new AtomicInt32();

        public static TMessage LastMessage { get; protected set; }

        public static int Remaining => _sync.CurrentCount;

        public static int Consumed => _consumed.Value;

        public static Task WaitAsync(int millisecondsTimeout) => _sync.WaitAsync(Debugger.IsAttached ? millisecondsTimeout * 10 : millisecondsTimeout);

        public static async Task ResetAsync()
        {
            while (Remaining > 0)
            {
                await _sync.WaitAsync(1000);
            }
            _consumed.Value = 0;
        }
    }

    public class TestObjectCreatedConsumer : TestObjectConsumerBase<TestObjectCreatedConsumer, TestObjectCreated>, IConsumer<TestObjectCreated>
    {
        public static AtomicInt32 Redeliveries { get; } = new AtomicInt32();

        public async Task Consume(IConsumeContext context, TestObjectCreated message)
        {
            //await context.Publish(new TestObjectUpdated { Id = message.Id });

            //if (context.Redeliveries < message.Redeliveries)
            //{
            //	Redeliveries.Increment();
            //	throw new Exception($"Retry {context.Retries} of {message.Retries}");
            //}

            _consumed.Increment();
            await context.Publish(new TestObjectUpdated { Id = message.Id });
            LastMessage = message;
            _sync.Release();
        }
    }

    public class TestObjectCreatedConsumer2 : TestObjectConsumerBase<TestObjectCreatedConsumer2, TestObjectCreated>, IConsumer<TestObjectCreated>
    {
        public static AtomicInt32 Retries { get; } = new AtomicInt32();

        public async Task Consume(IConsumeContext context, TestObjectCreated message)
        {
            await context.Publish(new TestObjectUpdated { Id = message.Id });

            if (context.Retries < message.Retries)
            {
                Retries.Increment();
                throw new Exception($"Retry {context.Retries} of {message.Retries}");
            }

            _consumed.Increment();
            LastMessage = message;
            _sync.Release();
        }
    }

    public class TestObjectUpdatedConsumer : TestObjectConsumerBase<TestObjectUpdatedConsumer, TestObjectUpdated>, IConsumer<TestObjectUpdated>
    {
        public Task Consume(IConsumeContext context, TestObjectUpdated message)
        {
            _consumed.Increment();
            LastMessage = message;
            _sync.Release();
            return Task.CompletedTask;
        }
    }

    public class TestObjectUpdated
    {
        public Guid Id { get; set; }
    }

    public class TestObjectCreated
    {
        public Guid Id { get; set; }
        public int Retries { get; set; }
        public int Redeliveries { get; set; }
    }
}
