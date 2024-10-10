using System;
using System.Collections.Generic;
using System.Reflection;

namespace FrankenBit.BoltWire;

internal static class SupportedCollectionTypes
{
    private const BindingFlags Binding = BindingFlags.Static | BindingFlags.NonPublic;
    
    internal static IReadOnlyCollection<Type> For<TService>() =>
        new[]
        {
            typeof(IEnumerable<TService>),
            typeof(IReadOnlyCollection<TService>),
            typeof(TService[])
        };

    internal static IReadOnlyCollection<Type> For(Type serviceType) =>
        (IReadOnlyCollection<Type>)GetGenericVariant(serviceType).Invoke(null, Array.Empty<object>());

    private static MethodInfo GetGenericVariant(Type serviceType) =>
        typeof(SupportedCollectionTypes).GetMethod(nameof(For), Binding, default, Type.EmptyTypes, default)!
            .MakeGenericMethod(serviceType);
}