using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common
{
    public static class Module<T>
    {
        private static readonly CowList<Func<T>> _resolvers = new CowList<Func<T>>();
        private static Func<T> _resolver;

        public static T Resolve()
        {
            var resolver = _resolver;
            if (resolver is null)
                throw new Exception($"Module<{typeof(T).FullName}> has not been registered.");
            return resolver();
        }

        public static IEnumerable<T> ResolveAll()
        {
            if (_resolvers.Count == 0)
                throw new Exception($"Module<{typeof(T).FullName}> has not been registered.");
            return _resolvers.Select(r => r());
        }

        public static void Register(T instance) => _resolvers.Add(_resolver = () => instance);
        public static void Register(Func<T> resolver) => _resolvers.Add(_resolver = resolver.NotNull(nameof(resolver)));

        public static void Clear()
        {
            _resolvers.Clear();
            _resolver = null;
        }
    }
}