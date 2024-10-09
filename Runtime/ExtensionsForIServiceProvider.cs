using System;
using System.Collections.Generic;
using System.Linq;
using FrankenBit.BoltWire.Exceptions;

namespace FrankenBit.BoltWire;

public static class ExtensionsForIServiceProvider
{
    public static TService Require<TService>(this IServiceProvider provider, string? key = default)
        where TService : class =>
        provider.Resolve<TService>(key) ?? throw ServiceNotRegisteredException.For<TService>();

    public static TService? Resolve<TService>(this IServiceProvider provider, string? key = default)
        where TService : class =>
        provider.GetService(typeof(TService), key) as TService;

    public static IEnumerable<TService> ResolveAll<TService>(this IServiceProvider provider, string? key = default)
        where TService : class =>
        provider.ResolveAll(typeof(TService), key).Cast<TService>();

    private static IEnumerable<object> ResolveAll(this IServiceProvider provider, Type serviceType,
        string? key = default) =>
        provider.GetService(typeof(IEnumerable<>).MakeGenericType(serviceType), key) as IEnumerable<object> ??
        Array.Empty<object>();
}
