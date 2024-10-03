using System;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class ServiceDoesNotImplementAnyInterfaces : ServiceRegistrationException
{
    private ServiceDoesNotImplementAnyInterfaces(Type type)
        : base(type, $"Service of type {type} does not implement any interfaces.")
    {
    }
    
    internal static ServiceDoesNotImplementAnyInterfaces For<TService>() =>
        new(typeof(TService));
}
