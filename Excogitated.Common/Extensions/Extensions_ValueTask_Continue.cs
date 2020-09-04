using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Extensions
{
    public static class Extensions_ValueTask_Continue
    {
        public static async ValueTask<R> Continue<T, R>(this ValueTask<T> task, Func<T, Task<R>> selector)
        {
            var value = await task;
            return await selector.NotNull(nameof(selector)).Invoke(value);
        }

        public static async ValueTask Continue<T>(this ValueTask<T> task, Func<T, Task> action)
        {
            var value = await task;
            await action.NotNull(nameof(action)).Invoke(value);
        }

        public static async ValueTask<R> Continue<R>(this ValueTask task, Func<Task<R>> selector)
        {
            await task;
            return await selector.NotNull(nameof(selector)).Invoke();
        }

        public static async ValueTask Continue(this ValueTask task, Func<Task> selector)
        {
            await task;
            await selector.NotNull(nameof(selector)).Invoke();
        }

        public static async ValueTask<R> Continue<T, R>(this ValueTask<T> task, Func<T, ValueTask<R>> selector)
        {
            var value = await task;
            return await selector.NotNull(nameof(selector)).Invoke(value);
        }

        public static async ValueTask Continue<T>(this ValueTask<T> task, Func<T, ValueTask> action)
        {
            var value = await task;
            await action.NotNull(nameof(action)).Invoke(value);
        }

        public static async ValueTask<R> Continue<R>(this ValueTask task, Func<ValueTask<R>> selector)
        {
            await task;
            return await selector.NotNull(nameof(selector)).Invoke();
        }

        public static async ValueTask Continue(this ValueTask task, Func<ValueTask> selector)
        {
            await task;
            await selector.NotNull(nameof(selector)).Invoke();
        }

        public static async ValueTask<R> Continue<T, R>(this ValueTask<T> task, Func<T, R> selector)
        {
            var value = await task;
            return selector.NotNull(nameof(selector)).Invoke(value);
        }

        public static async ValueTask Continue<T>(this ValueTask<T> task, Action<T> action)
        {
            var value = await task;
            action.NotNull(nameof(action)).Invoke(value);
        }

        public static async ValueTask<R> Continue<R>(this ValueTask task, Func<R> selector)
        {
            await task;
            return selector.NotNull(nameof(selector)).Invoke();
        }

        public static async ValueTask Continue(this ValueTask task, Action action)
        {
            await task;
            action.NotNull(nameof(action)).Invoke();
        }
    }
}
