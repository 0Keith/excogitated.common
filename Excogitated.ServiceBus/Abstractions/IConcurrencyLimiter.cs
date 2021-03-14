using System;
using System.Threading.Tasks;

namespace Excogitated.ServiceBus.Abstractions
{
    public interface IConcurrencyLimiter
    {
        ValueTask<IDisposable> AcquirePublishSlot();
        ValueTask<IDisposable> AcquireConsumerSlot();
    }
}