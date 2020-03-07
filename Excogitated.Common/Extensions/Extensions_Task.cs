using System;
using System.Threading.Tasks;

namespace Excogitated.Common
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

    public static class Extensions_Task_Continue
    {
        public static ValueTask<T> ToValueTask<T>(this Task<T> task) => new ValueTask<T>(task);
        public static ValueTask ToValueTask(this Task task) => new ValueTask(task);

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

    public static class Extensions_Task
    {
        public static Task<T> OrDefault<T>(this Task<T> task) => task ?? Task.FromResult(default(T));
        public static Task OrDefault(this Task task) => task ?? Task.CompletedTask;

        public static async void Catch(this ILogger logger, Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                logger?.Error(e);
            }
        }

        public static async void Catch(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Loggers.Error(e);
            }
        }

        public static async void Catch(this ValueTask task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Loggers.Error(e);
            }
        }

        public static async void Catch<T>(this ValueTask<T> task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Loggers.Error(e);
            }
        }
    }
}
