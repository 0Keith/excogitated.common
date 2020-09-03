using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Counting
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var time = Stopwatch.StartNew();
            var ticks = Stopwatch.Frequency * 30;
            var counts = await Task.WhenAll(Enumerable.Range(0, 4).Select(i => Task.Run(() =>
            {
                var count = 0L;
                while (ticks > time.ElapsedTicks)
                    count++;
                return count;
            })));
            Console.WriteLine(new
            {
                Avg = counts.Average().ToString("n"),
                AvgPerSecond = (counts.Average() / 30).ToString("n"),

                Total = counts.Sum().ToString("n"),
                TotalPerSecond = (counts.Sum() / 30).ToString("n")
            });
            Console.WriteLine("Press 'Enter' to Exit...");
            await Console.In.ReadLineAsync();
        }
    }
}
