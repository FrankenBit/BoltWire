using System;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class CompositeAlreadySetException : ServiceRegistrationException
{
    private CompositeAlreadySetException(Type serviceType)
        : base(serviceType, $"Composite for service {serviceType} is already set.")
    {
    }

    internal static CompositeAlreadySetException For<TService>() =>
        new(typeof(TService));
}
