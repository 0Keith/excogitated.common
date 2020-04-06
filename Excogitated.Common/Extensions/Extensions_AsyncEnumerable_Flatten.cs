using System.Collections.Generic;

namespace Excogitated.Common
{
    public static class Extensions_AsyncEnumerable_Flatten
    {
        public static async IAsyncEnumerable<T> Flatten<T>(this IAsyncEnumerable<IAsyncEnumerable<T>> source)
        {
            if (source.IsNotNull())
                await foreach (var items in source)
                    if (items.IsNotNull())
                        await foreach (var item in items)
                            yield return item;
        }

        public static async IAsyncEnumerable<T> Flatten<T>(this IAsyncEnumerable<IEnumerable<T>> source)
        {
            if (source.IsNotNull())
                await foreach (var items in source)
                    if (items.IsNotNull())
                        foreach (var item in items)
                            yield return item;
        }

        public static async IAsyncEnumerable<T> Flatten<T>(this IEnumerable<IAsyncEnumerable<T>> source)
        {
            if (source.IsNotNull())
                foreach (var items in source)
                    if (items.IsNotNull())
                        await foreach (var item in items)
                            yield return item;
        }
    }
}