using System;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class CircularDependencyException : ServiceCompositionException
{
    internal CircularDependencyException(Type serviceType, Type dependencyType)
        : base(serviceType, dependencyType, $"Recursive composition detected for service of type {dependencyType}.")
    {
    }
}