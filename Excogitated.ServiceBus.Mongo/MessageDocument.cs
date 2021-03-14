using System;

namespace Excogitated.ServiceBus.Mongo
{
    internal class MessageDocument
    {
        public Guid Id { get; set; }
        public byte[] Data { get; set; }
        public DateTimeOffset PublishedAt { get; set; }
        public string MessageType { get; set; }
        public DateTimeOffset LockExpiration { get; set; }
    }
}