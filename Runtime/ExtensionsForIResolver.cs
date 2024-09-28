using System;
using System.Collections.Generic;
using System.Linq;
using FrankenBit.BoltWire.Exceptions;

namespace FrankenBit.BoltWire;

public static class ExtensionsForIResolver
{
        
    public static TService Require<TService>(this IServiceProvider resolver) where TService : class =>
        resolver.Resolve<TService>() ?? throw ServiceNotRegisteredException.For<TService>();

    public static TService? Resolve<TService>(this IServiceProvider resolver) where TService : class =>
        resolver.GetService(typeof(TService)) as TService;

    public static IEnumerable<TService> ResolveAll<TService>(this IServiceProvider resolver)
        where TService : class =>
        resolver.ResolveAll(typeof(TService)).Cast<TService>();

    private static IEnumerable<object>
        ResolveAll(this IServiceProvider resolver, Type serviceType) =>
        resolver.GetService(typeof(IEnumerable<>).MakeGenericType(serviceType)) as IEnumerable<object> ??
        Array.Empty<object>();
}