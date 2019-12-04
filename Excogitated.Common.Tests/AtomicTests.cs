using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common.Test
{
    [TestClass]
    public class AtomicTests
    {
        [TestMethod]
        public async Task Delay()
        {
            var delays = Enumerable.Range(0, 1000)
                .Select(i => AsyncTimer.Delay(Rng.Pseudo.GetInt32(1, 1000)))
                .ToList();
            foreach (var delay in delays)
            {
                var result = await delay;
                Assert.AreEqual(0, result, 30);
            }
        }
    }
}
