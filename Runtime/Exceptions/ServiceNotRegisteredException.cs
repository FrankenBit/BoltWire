using System;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class ServiceNotRegisteredException : ServiceResolutionException
{
    private ServiceNotRegisteredException(Type type)
        : base(type, $"Service of type {type} is not registered.")
    {
    }

    internal static ServiceNotRegisteredException For<TService>() =>
        new(typeof(TService));
}