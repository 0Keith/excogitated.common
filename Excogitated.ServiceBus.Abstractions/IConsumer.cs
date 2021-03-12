using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IConsumer { }
    public interface IConsumer<T> : IConsumer where T : class
    {
        ValueTask Consume(IConsumeContext context, T message);
    }
}
