using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Extensions
{
    public static class Extensions_Task_Continue
    {
        public static ValueTask<T> ToValueTask<T>(this Task<T> task) => new(task);
        public static ValueTask ToValueTask(this Task task) => new(task);

        public static async Task<R> Continue<T, R>(this Task<T> task, Func<T, Task<R>> selector)
        {
            var value = await task.NotNull(nameof(task));
            return await selector.NotNull(nameof(selector)).Invoke(value);
        }

        public static async Task Continue<T>(this Task<T> task, Func<T, Task> action)
        {
            var value = await task.NotNull(nameof(task));
            await action.NotNull(nameof(action)).Invoke(value);
        }

        public static async Task<R> Continue<R>(this Task task, Func<Task<R>> selector)
        {
            await task.NotNull(nameof(task));
            return await selector.NotNull(nameof(selector)).Invoke();
        }

        public static async Task Continue(this Task task, Func<Task> selector)
        {
            await task.NotNull(nameof(task));
            await selector.NotNull(nameof(selector)).Invoke();
        }

        public static async Task<R> Continue<T, R>(this Task<T> task, Func<T, ValueTask<R>> selector)
        {
            var value = await task.NotNull(nameof(task));
            return await selector.NotNull(nameof(selector)).Invoke(value);
        }

        public static async Task Continue<T>(this Task<T> task, Func<T, ValueTask> action)
        {
            var value = await task.NotNull(nameof(task));
            await action.NotNull(nameof(action)).Invoke(value);
        }

        public static async Task<R> Continue<R>(this Task task, Func<ValueTask<R>> selector)
        {
            await task.NotNull(nameof(task));
            return await selector.NotNull(nameof(selector)).Invoke();
        }

        public static async Task Continue(this Task task, Func<ValueTask> selector)
        {
            await task.NotNull(nameof(task));
            await selector.NotNull(nameof(selector)).Invoke();
        }

        public static async Task<R> Continue<T, R>(this Task<T> task, Func<T, R> selector)
        {
            var value = await task.NotNull(nameof(task));
            return selector.NotNull(nameof(selector)).Invoke(value);
        }

        public static async Task Continue<T>(this Task<T> task, Action<T> action)
        {
            var value = await task.NotNull(nameof(task));
            action.NotNull(nameof(action)).Invoke(value);
        }

        public static async Task<R> Continue<R>(this Task task, Func<R> selector)
        {
            await task.NotNull(nameof(task));
            return selector.NotNull(nameof(selector)).Invoke();
        }

        public static async Task Continue(this Task task, Action action)
        {
            await task.NotNull(nameof(task));
            action.NotNull(nameof(action)).Invoke();
        }
    }
}
