using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IConsumer { }
    public interface IConsumer<T> : IConsumer where T : class
    {
        Task Consume(IConsumeContext context, T message);
    }
}
