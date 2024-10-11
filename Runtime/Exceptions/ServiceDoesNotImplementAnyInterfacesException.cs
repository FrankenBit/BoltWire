using System;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class ServiceDoesNotImplementAnyInterfacesException : ServiceRegistrationException
{
    private ServiceDoesNotImplementAnyInterfacesException(Type type)
        : base(type, $"Service of type {type} does not implement any interfaces.")
    {
    }
    
    internal static ServiceDoesNotImplementAnyInterfacesException For<TService>() =>
        new(typeof(TService));
}
