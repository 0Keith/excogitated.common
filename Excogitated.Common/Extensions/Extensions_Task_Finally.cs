using System;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public static class Extensions_Task_Finally
    {
        public static async ValueTask Finally(this Task task, Action action)
        {
            try
            {
                await task.NotNull(nameof(task));
            }
            finally
            {
                action.NotNull(nameof(action)).Invoke();
            }
        }

        public static async ValueTask<T> Finally<T>(this Task<T> task, Action action)
        {
            try
            {
                return await task.NotNull(nameof(task));
            }
            finally
            {
                action.NotNull(nameof(action)).Invoke();
            }
        }

        public static async ValueTask Finally(this Task task, Func<ValueTask> action)
        {
            try
            {
                await task.NotNull(nameof(task));
            }
            finally
            {
                await action.NotNull(nameof(action)).Invoke();
            }
        }

        public static async ValueTask Finally(this Task task, Func<Task> selector)
        {
            try
            {
                await task.NotNull(nameof(task));
            }
            finally
            {
                await selector.NotNull(nameof(selector)).Invoke();
            }
        }

        public static async ValueTask<T> Finally<T>(this Task<T> task, Func<ValueTask> selector)
        {
            try
            {
                return await task.NotNull(nameof(task));
            }
            finally
            {
                await selector.NotNull(nameof(selector)).Invoke();
            }
        }

        public static async ValueTask<T> Finally<T>(this Task<T> task, Func<Task> selector)
        {
            try
            {
                return await task.NotNull(nameof(task));
            }
            finally
            {
                await selector.NotNull(nameof(selector)).Invoke();
            }
        }
    }
}
