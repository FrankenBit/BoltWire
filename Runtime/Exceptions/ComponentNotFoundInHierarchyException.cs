using System;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class ComponentNotFoundInHierarchyException : ServiceRegistrationException
{
    private ComponentNotFoundInHierarchyException(Type serviceType)
        : base(serviceType, $"{serviceType.Name} component not found in hierarchy.")
    {
    }

    internal static Exception For<TService>() =>
        new ComponentNotFoundInHierarchyException(typeof(TService));
}
