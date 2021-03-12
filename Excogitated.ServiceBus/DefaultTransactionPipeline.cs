using Excogitated.ServiceBus.Abstractions;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace Excogitated.ServiceBus
{
    internal class DefaultTransactionPipeline : IConsumerPipeline
    {
        public IConsumerPipeline Pipeline { get; }

        public DefaultTransactionPipeline(IConsumerPipeline pipeline)
        {
            Pipeline = pipeline;
        }

        public async ValueTask Execute(IConsumeContext context, BinaryData message, ConsumerDefinition definition)
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var transactionContext = new DefaultTransactionConsumeContext(context);
            await Pipeline.Execute(transactionContext, message, definition);
            await transactionContext.PublishMessages();
            transaction.Complete();
        }
    }
}
