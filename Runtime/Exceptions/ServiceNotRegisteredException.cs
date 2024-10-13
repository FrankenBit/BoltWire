using System;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class ServiceNotRegisteredException : ServiceResolutionException
{
    private ServiceNotRegisteredException(Type serviceType)
        : base(serviceType, $"{serviceType.Name} is not registered.")
    {
    }

    internal static ServiceNotRegisteredException For<TService>() =>
        new(typeof(TService));
}
