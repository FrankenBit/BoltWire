using System;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class ComponentNotFoundInHierarchyException : ServiceRegistrationException
{
    private ComponentNotFoundInHierarchyException(Type serviceType)
        : base(serviceType, $"Component of type {serviceType} not found in hierarchy")
    {
    }

    internal static Exception Create<TService>() =>
        new ComponentNotFoundInHierarchyException(typeof(TService));
}