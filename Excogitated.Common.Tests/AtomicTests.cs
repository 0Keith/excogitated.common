using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
            var deltas = await Task.WhenAll(Enumerable.Range(0, 1000)
                .Select(i => Rng.Pseudo.GetInt32(1, 1000))
                .Select(rng => new
                {
                    Delay = AsyncTimer.Delay(rng),
                    Expected = DateTime.Now.AddMilliseconds(rng)
                })
                .Select(r => r.Delay.Continue(delta =>
                {
                    Assert.AreEqual(0, delta, 10);
                    Assert.AreEqual(0, (r.Expected - DateTime.Now).TotalMilliseconds, 10);
                    return delta;
                })));
            Console.WriteLine($"Min Delta: {deltas.Min()}");
            Console.WriteLine($"Max Delta: {deltas.Max()}");
            Console.WriteLine($"Average Delta: {deltas.Average()}");
        }
    }
}
