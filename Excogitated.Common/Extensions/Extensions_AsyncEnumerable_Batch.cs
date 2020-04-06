using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public static class Extensions_AsyncEnumerable_Batch
    {

        public static ValueTask Batch<T>(this IAsyncEnumerable<T> source, Func<T, ValueTask> action) => source.Batch(0, action);
        public static async ValueTask Batch<T>(this IAsyncEnumerable<T> source, int threadCount, Func<T, ValueTask> action)
        {
            if (threadCount <= 0)
                threadCount = Environment.ProcessorCount / 2;
            action.NotNull(nameof(action));
            var @lock = new AsyncLock();
            await using var items = source.NotNull(nameof(source)).GetAsyncEnumerator();
            await Enumerable.Range(0, threadCount).Select(i => Task.Run(async () =>
            {
                while (true)
                {
                    T item;
                    using (await @lock.EnterAsync())
                    {
                        if (!await items.MoveNextAsync())
                            return;
                        item = items.Current;
                    }
                    try
                    {
                        await action(item);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Batch failed on item: {item}", e);
                    }
                }
            })).WhenAll();
        }

        public static IAsyncEnumerable<R> Batch<T, R>(this IAsyncEnumerable<T> source, Func<T, ValueTask<R>> action) => source.Batch(0, action);
        public static async IAsyncEnumerable<R> Batch<T, R>(this IAsyncEnumerable<T> source, int threadCount, Func<T, ValueTask<R>> action)
        {
            action.NotNull(nameof(action));
            var results = new AsyncQueue<R>();
            var batch = source.Batch(threadCount, async i => results.Add(await action(i)))
                .Finally(() => results.Complete());
            try
            {
                await foreach (var result in results.ConsumeAsync())
                    yield return result;
            }
            finally
            {
                await batch;
            }
        }
    }
}