using System.Threading.Tasks;

namespace Excogitated.Common.Extensions
{
    public static class Extensions_Task
    {
        public static Task<T> OrDefault<T>(this Task<T> task) => task ?? Task.FromResult(default(T));
        public static Task OrDefault(this Task task) => task ?? Task.CompletedTask;
    }
}
