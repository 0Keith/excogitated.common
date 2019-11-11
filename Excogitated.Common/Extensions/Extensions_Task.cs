using System;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public static class Extensions_Task
    {
        public static async ValueTask<TReturn> Select<TSource, TReturn>(this ValueTask<TSource> task, Func<TSource, TReturn> selector)
        {
            selector.NotNull(nameof(selector));
            var result = await task;
            return selector(result);
        }

        public static Task<T> OrDefault<T>(this Task<T> task) => task ?? Task.FromResult(default(T));
        public static Task OrDefault(this Task task) => task ?? Task.CompletedTask;
    }
}
