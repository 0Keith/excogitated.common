using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IConsumeContext
    {
        int Retries { get; }
        int Attempts { get; }
        int Redeliveries { get; }
        DateTimeOffset InitialDeliveryDate { get; }

        Task Publish<T>(T message) where T : class;
        Task Reschedule(DateTimeOffset deliveryDate);
    }
}
