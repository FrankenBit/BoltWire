using System;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class HiddenDisposableRegistrationException : ServiceRegistrationException
{
    private HiddenDisposableRegistrationException(Type serviceType, Type implementationType)
        : base(serviceType,
        $"Transient registration of {implementationType.Name} as {serviceType.Name} would hide IDisposable.") =>
        ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));

    public Type ImplementationType { get; }

    internal static HiddenDisposableRegistrationException For<TService, TImplementation>()
        where TImplementation : TService =>
        new(typeof(TService), typeof(TImplementation));
}
