using System;

namespace FrankenBit.BoltWire.Exceptions;

public abstract class ServiceCompositionException : ServiceResolutionException
{
    protected ServiceCompositionException(Type serviceType, Type dependencyType,
        string message)
        : base(serviceType, message) =>
        DependencyType = dependencyType ?? throw new ArgumentNullException(nameof(dependencyType));

    protected ServiceCompositionException(Type serviceType, Type dependencyType,
        string message, Exception innerException)
        : base(serviceType, message, innerException) =>
        DependencyType = dependencyType;

    public Type DependencyType { get; }
}