using System;
using System.Collections.Generic;
using System.Linq;

namespace Excogitated.Common
{
    /// <summary>
    /// A micro IOC framework. Offers fastest resolve possible but with minimal features.
    /// </summary>
    /// <typeparam name="T">Specifies the Type to Register or Resolve</typeparam>
    public static class Module<T>
    {
        private static readonly CowList<Func<T>> _resolvers = new CowList<Func<T>>();
        private static Func<T> _resolver;

        /// <summary>
        /// Resolves a single instance of the specified Type using the last registered resolver.
        /// </summary>
        /// <returns>The resolved instance.</returns>
        public static T Resolve()
        {
            var resolver = _resolver;
            if (resolver is null)
                throw new Exception($"Module<{typeof(T).FullName}> has not been registered.");
            return resolver();
        }

        /// <summary>
        /// Resolves an instance of the specified Type using all registered resolvers.
        /// </summary>
        /// <returns>The resolved instances.</returns>
        public static IEnumerable<T> ResolveAll()
        {
            if (_resolvers.Count == 0)
                throw new Exception($"Module<{typeof(T).FullName}> has not been registered.");
            return _resolvers.Select(r => r());
        }

        /// <summary>
        /// Register a singleton instance.
        /// </summary>
        /// <param name="instance">The instance to register.</param>
        public static void Register(T instance) => _resolvers.Add(_resolver = () => instance);

        /// <summary>
        /// Register a delegate as a resolver. The delegate will be invoked everytime Resolve() or ResolveAll() is called.
        /// </summary>
        /// <param name="resolver">The delegate that will return an instance of the specificed Type.</param>
        public static void Register(Func<T> resolver) => _resolvers.Add(_resolver = resolver.NotNull(nameof(resolver)));

        /// <summary>
        /// Unregister all resolvers for the specified Type.
        /// </summary>
        public static void Clear()
        {
            _resolvers.Clear();
            _resolver = null;
        }
    }
}