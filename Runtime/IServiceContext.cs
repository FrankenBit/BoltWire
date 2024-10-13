using System;
using System.Diagnostics.CodeAnalysis;

namespace FrankenBit.BoltWire;

internal interface IServiceContext
{
    string? Key { get; }
    object GetDependency(Type serviceType, Type dependencyType, string? key);
    TService Track<TService>(TService service, ServiceLifetime lifetime) where TService : class;
    bool TryGetInstance(Type implementationType, [NotNullWhen(true)] out object? instance);
}
