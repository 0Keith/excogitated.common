using System;
using System.Linq.Expressions;

namespace Excogitated.Common.Extensions
{
    public static class Extensions_Expression
    {
        public static string GetName<P, V>(this Expression<Func<P, V>> property)
        {
            if (property?.Body is MemberExpression m)
                return m.Member.Name;
            throw new ArgumentException("is not a MemberExpression", nameof(property));
        }

        public static string GetFullName<P, V>(this Expression<Func<P, V>> property)
        {
            return $"{typeof(P).FullName}:{property.GetName()}";
        }
    }
}
