using System;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class FactoryReturnedNullException : ServiceResolutionException
{
    internal FactoryReturnedNullException(Type serviceType)
        : base(serviceType, $"Factory for {serviceType} returned null.")
    {
    }
        
    internal static FactoryReturnedNullException For<TService>() =>
        new(typeof(TService));
}
