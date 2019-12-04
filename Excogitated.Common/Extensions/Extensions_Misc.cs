using System.Diagnostics;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public static class Extensions_Misc
    {
        public static async Task Pause(this Stopwatch watch, int minDuration = 2500, int maxDuration = 5000)
        {
            watch?.Stop();
            await AsyncTimer.Delay(Rng.Pseudo.GetInt32(minDuration, maxDuration));
            watch?.Start();
        }
    }
}