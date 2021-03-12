using System;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IServiceBusSerializer
    {
        //object Deserialize(BinaryData message, Type messageType);
        BinaryData Serialize<T>(T message) where T : class;
        T Deserialize<T>(BinaryData message);
    }
}
