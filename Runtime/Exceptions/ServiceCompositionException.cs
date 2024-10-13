using System;

namespace FrankenBit.BoltWire.Exceptions;

public abstract class ServiceCompositionException : ServiceResolutionException
{
    protected ServiceCompositionException(Type serviceType, Type dependencyType,
        string message)
        : base(serviceType, message) =>
        DependencyType = dependencyType ?? throw new ArgumentNullException(nameof(dependencyType));

    public Type DependencyType { get; }
}
