using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public static class Extensions_AsyncEnumerable_Select
    {
        public static async IAsyncEnumerable<R> Select<T, R>(this IAsyncEnumerable<T> source, Func<T, R> selector)
        {
            source.NotNull(nameof(source));
            selector.NotNull(nameof(selector));
            await foreach (var item in source)
                yield return selector(item);
        }

        //public static IAsyncEnumerable<R> Select<T, R>(this IAsyncEnumerable<T> source, Func<T, Task<R>> selector) => source.Select(selector.ToAsync());
        public static async IAsyncEnumerable<R> Select<T, R>(this IAsyncEnumerable<T> source, Func<T, ValueTask<R>> selector)
        {
            source.NotNull(nameof(source));
            selector.NotNull(nameof(selector));
            await foreach (var item in source)
                yield return await selector(item);
        }

        public static async IAsyncEnumerable<R> SelectMany<T, R>(this IAsyncEnumerable<T> source, Func<T, IEnumerable<R>> selector)
        {
            source.NotNull(nameof(source));
            selector.NotNull(nameof(selector));
            await foreach (var item in source)
                foreach (var many in selector(item))
                    yield return many;
        }

        public static async IAsyncEnumerable<R> SelectMany<T, R>(this IAsyncEnumerable<T> source, Func<T, ValueTask<IEnumerable<R>>> selector)
        {
            source.NotNull(nameof(source));
            selector.NotNull(nameof(selector));
            await foreach (var item in source)
                foreach (var many in await selector(item))
                    yield return many;
        }

        public static async IAsyncEnumerable<R> SelectMany<T, R>(this IAsyncEnumerable<T> source, Func<T, IAsyncEnumerable<R>> selector)
        {
            source.NotNull(nameof(source));
            selector.NotNull(nameof(selector));
            await foreach (var item in source)
                await foreach (var many in selector(item))
                    yield return many;
        }
    }
}