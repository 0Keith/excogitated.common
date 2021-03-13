using Excogitated.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Extensions
{
    public static class Extensions_HashSet
    {
        public static bool[] AddRange<T>(this HashSet<T> target, IEnumerable<T> source)
        {
            if (target is null || source is null)
                return new bool[0];
            return source.Select(target.Add).ToArray();
        }
    }
}
