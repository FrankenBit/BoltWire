using System;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class UnresolvedDependencyException : ServiceCompositionException
{
    internal UnresolvedDependencyException(Type serviceType, Type dependencyType)
        : base(serviceType, dependencyType,
        $"{serviceType.Name} dependency {dependencyType.Name} could not be resolved.")
    {
    }

    internal static UnresolvedDependencyException For<TService, TDependency>() =>
        new(typeof(TService), typeof(TDependency));
}
