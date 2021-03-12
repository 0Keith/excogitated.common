using System;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IServiceBusSerializer
    {
        BinaryData Serialize<T>(T message) where T : class;
        T Deserialize<T>(BinaryData message);
    }
}
