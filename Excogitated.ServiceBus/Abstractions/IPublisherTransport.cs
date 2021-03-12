using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IPublisherTransport
    {
        Task Configure(PublisherDefinition definition);
        Task Publish(BinaryData message);
    }
}
