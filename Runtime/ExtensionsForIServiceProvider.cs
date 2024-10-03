using System;
using System.Collections.Generic;
using System.Linq;
using FrankenBit.BoltWire.Exceptions;

namespace FrankenBit.BoltWire;

public static class ExtensionsForIServiceProvider
{
    public static IScope CreateScope(this IServiceProvider container) =>
        throw new NotImplementedException();
    
    public static TService Require<TService>(this IServiceProvider provider) where TService : class =>
        provider.Resolve<TService>() ?? throw ServiceNotRegisteredException.For<TService>();

    public static TService? Resolve<TService>(this IServiceProvider provider) where TService : class =>
        provider.GetService(typeof(TService)) as TService;

    public static IEnumerable<TService> ResolveAll<TService>(this IServiceProvider provider)
        where TService : class =>
        provider.ResolveAll(typeof(TService)).Cast<TService>();

    private static IEnumerable<object>
        ResolveAll(this IServiceProvider provider, Type serviceType) =>
        provider.GetService(typeof(IEnumerable<>).MakeGenericType(serviceType)) as IEnumerable<object> ??
        Array.Empty<object>();
}
