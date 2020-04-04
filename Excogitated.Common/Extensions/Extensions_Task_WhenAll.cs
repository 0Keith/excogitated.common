using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public static class Extensions_Task_WhenAll
    {
        public static async ValueTask WhenAll(this IEnumerable<Task> tasks)
        {
            var taskList = tasks.NotNull(nameof(tasks)).ToList();
            foreach (var task in taskList)
                if (task.IsNotNull())
                    await task;
        }

        public async static ValueTask WhenAll(this IEnumerable<ValueTask> tasks)
        {
            var taskList = tasks.NotNull(nameof(tasks)).ToList();
            foreach (var task in taskList)
                await task;
        }

        public static async ValueTask<List<T>> WhenAll<T>(this IEnumerable<Task<T>> tasks)
        {
            var results = new List<T>();
            var taskList = tasks.NotNull(nameof(tasks)).ToList();
            foreach (var task in taskList)
                if (task.IsNotNull())
                    results.Add(await task);
            return results;
        }

        public static async ValueTask<List<T>> WhenAll<T>(this IEnumerable<ValueTask<T>> tasks)
        {
            var results = new List<T>();
            var taskList = tasks.NotNull(nameof(tasks)).ToList();
            foreach (var task in taskList)
                results.Add(await task);
            return results;
        }
    }
}
