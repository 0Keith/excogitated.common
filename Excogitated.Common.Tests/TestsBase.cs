using Excogitated.Common.Atomic.Collections;
using Excogitated.Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Excogitated.Tests
{
    public abstract class TestsBase
    {
        private TestLogger _logger;

        [TestInitialize]
        public void Initialize()
        {
            _logger = Loggers.Register(new TestLogger());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _logger.AssertSuccessful();
        }
    }

    internal class TestLogger : ILogger
    {
        private readonly AtomicQueue<LogMessage> _messages = new();

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
