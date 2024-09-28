using System;
using System.Collections.Generic;
using System.Linq;
using FrankenBit.BoltWire.Exceptions;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    public static class ExtensionsForIResolver
    {
        
        [NotNull]
        public static TService Require<TService>([NotNull] this IServiceProvider resolver) where TService : class =>
            resolver.Resolve<TService>() ?? throw ServiceNotRegisteredException.For<TService>();

        [CanBeNull]
        public static TService Resolve<TService>([NotNull] this IServiceProvider resolver) where TService : class =>
            resolver.GetService(typeof(TService)) as TService;

        [ItemNotNull]
        [NotNull]
        public static IEnumerable<TService> ResolveAll<TService>([NotNull] this IServiceProvider resolver)
            where TService : class =>
            resolver.ResolveAll(typeof(TService)).Cast<TService>();

        [ItemNotNull]
        [NotNull]
        private static IEnumerable<object>
            ResolveAll([NotNull] this IServiceProvider resolver, [NotNull] Type serviceType) =>
            resolver.GetService(typeof(IEnumerable<>).MakeGenericType(serviceType)) as IEnumerable<object> ??
            Array.Empty<object>();
    }
}
