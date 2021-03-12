using Excogitated.ServiceBus.Abstractions;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Excogitated.ServiceBus
{
    internal class DefaultSerializer : IServiceBusSerializer
    {
        T IServiceBusSerializer.Deserialize<T>(BinaryData message)
        {
            using var stream = message.ToStream();
            using var reader = new StreamReader(stream);
            using var json = new JsonTextReader(reader);
            var consumerMessage = JsonSerializer.CreateDefault().Deserialize<T>(json);
            return consumerMessage;
        }

        BinaryData IServiceBusSerializer.Serialize<T>(T message)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            using var json = new JsonTextWriter(writer);
            JsonSerializer.CreateDefault().Serialize(json, message);
            json.Flush();
            return BinaryData.FromBytes(stream.ToArray());
        }
    }
}
