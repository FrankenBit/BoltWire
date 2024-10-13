using System;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class ServiceDoesNotImplementAnyInterfacesException : ServiceRegistrationException
{
    private ServiceDoesNotImplementAnyInterfacesException(Type serviceType)
        : base(serviceType, $"{serviceType.Name} does not implement any interfaces.")
    {
    }
    
    internal static ServiceDoesNotImplementAnyInterfacesException For<TService>() =>
        new(typeof(TService));
}
