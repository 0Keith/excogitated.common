using System;
using System.Threading.Tasks;

namespace Excogitated.Common.Extensions
{
    public static class Extensions_ValueTask_Finally
    {
        public static async ValueTask Finally(this ValueTask task, Action action)
        {
            try
            {
                await task;
            }
            finally
            {
                action.NotNull(nameof(action)).Invoke();
            }
        }

        public static async ValueTask<T> Finally<T>(this ValueTask<T> task, Action action)
        {
            try
            {
                return await task;
            }
            finally
            {
                action.NotNull(nameof(action)).Invoke();
            }
        }

        public static async ValueTask Finally(this ValueTask task, Func<ValueTask> action)
        {
            try
            {
                await task;
            }
            finally
            {
                await action.NotNull(nameof(action)).Invoke();
            }
        }

        public static async ValueTask Finally(this ValueTask task, Func<Task> selector)
        {
            try
            {
                await task;
            }
            finally
            {
                await selector.NotNull(nameof(selector)).Invoke();
            }
        }

        public static async ValueTask<T> Finally<T>(this ValueTask<T> task, Func<ValueTask> selector)
        {
            try
            {
                return await task;
            }
            finally
            {
                await selector.NotNull(nameof(selector)).Invoke();
            }
        }

        public static async ValueTask<T> Finally<T>(this ValueTask<T> task, Func<Task> selector)
        {
            try
            {
                return await task;
            }
            finally
            {
                await selector.NotNull(nameof(selector)).Invoke();
            }
        }
    }
}
