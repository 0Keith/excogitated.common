using Excogitated.ServiceBus.Abstractions;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace Excogitated.ServiceBus
{
    internal class DefaultTransactionPipeline : IConsumerPipeline
    {
        private readonly IConsumerPipeline _pipeline;

        public DefaultTransactionPipeline(IConsumerPipeline pipeline)
        {
            _pipeline = pipeline;
        }

        public async Task Execute(IConsumeContext context, BinaryData message, ConsumerDefinition definition)
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var transactionContext = new TransactionConsumeContext(context);
            await _pipeline.Execute(transactionContext, message, definition);
            await transactionContext.PublishMessages();
            transaction.Complete();
        }
    }
}
