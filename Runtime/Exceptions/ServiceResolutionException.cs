using System;

namespace FrankenBit.BoltWire.Exceptions;

public abstract class ServiceResolutionException : ContainerException
{
    protected ServiceResolutionException(Type serviceType, string message)
        : base(serviceType, message)
    {
    }
}
