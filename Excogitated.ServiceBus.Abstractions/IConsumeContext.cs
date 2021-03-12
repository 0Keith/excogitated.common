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

        ValueTask Publish<T>(T message) where T : class;
        ValueTask Reschedule(DateTimeOffset deliveryDate);
    }
}
