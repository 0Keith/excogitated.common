using Excogitated.Common.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common.Extensions
{
    public static partial class Extensions_Enumerable
    {
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> items) => items.OrderBy(i => Rng.GetInt32());
    }
}
