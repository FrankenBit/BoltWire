using System;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class CompositeAlreadySetException : ServiceRegistrationException
{
    private CompositeAlreadySetException(Type serviceType)
        : base(serviceType, $"Composite for {serviceType.Name} is already set.")
    {
    }

    internal static CompositeAlreadySetException For<TService>() =>
        new(typeof(TService));
}
