using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
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
            var watch = Stopwatch.StartNew();
            var deltas = await Task.WhenAll(Enumerable.Range(0, 1000)
                .Select(i => Rng.Pseudo.GetInt32(1, 1000))
                .Select(rng => new
                {
                    Delay = AsyncTimer.Delay(rng),
                    Expected = watch.ElapsedMilliseconds + rng
                })
                .Select(r => r.Delay.Continue(delta =>
                {
                    Assert.AreEqual(0, delta, 100);
                    Assert.AreEqual(0, r.Expected - watch.ElapsedMilliseconds, 100);
                    return delta;
                })));
            Console.WriteLine($"Min Delta: {deltas.Min()}");
            Console.WriteLine($"Max Delta: {deltas.Max()}");
            Console.WriteLine($"Average Delta: {deltas.Average()}");
        }
    }
}
