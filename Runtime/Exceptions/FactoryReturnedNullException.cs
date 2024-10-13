using System;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class FactoryReturnedNullException : ServiceResolutionException
{
    private FactoryReturnedNullException(Type serviceType)
        : base(serviceType, $"Factory for {serviceType.Name} returned null.")
    {
    }
        
    internal static FactoryReturnedNullException For<TService>() =>
        new(typeof(TService));
}
