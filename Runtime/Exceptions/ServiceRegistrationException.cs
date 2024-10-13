using System;

namespace FrankenBit.BoltWire.Exceptions;

public abstract class ServiceRegistrationException : ContainerException
{
    protected ServiceRegistrationException(Type serviceType, string message)
        : base(serviceType, message)
    {
    }
}
