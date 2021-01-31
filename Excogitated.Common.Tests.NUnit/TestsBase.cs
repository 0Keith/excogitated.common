using Excogitated.Common.Atomic.Collections;
using Excogitated.Common.Logging;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Excogitated.Common.Tests
{
    public abstract class TestsBase
    {
        private TestLogger _logger;

        [OneTimeSetUp]
        public void Initialize()
        {
            _logger = Loggers.Register(new TestLogger());
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _logger.AssertSuccessful();
        }
    }

    internal class TestLogger : ILogger
    {
        private readonly AtomicQueue<LogMessage> _messages = new AtomicQueue<LogMessage>();

        public Task ClearLog() => null;

        public void Log(LogMessage message) => _messages.Add(message);

        public void AssertSuccessful()
        {
            foreach (var msg in _messages.Consume())
                if (msg.Level == LogLevel.Error)
                    Assert.Fail(msg.ToString());
        }
    }
}
