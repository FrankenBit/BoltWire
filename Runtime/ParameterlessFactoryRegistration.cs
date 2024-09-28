using System;
using System.Collections.Generic;
using FrankenBit.BoltWire.Exceptions;

namespace FrankenBit.BoltWire;

internal sealed class ParameterlessFactoryRegistration<TService> : IRegistration
{
    private readonly Func<TService> _factory;

    internal ParameterlessFactoryRegistration(Func<TService>? factory, ServiceLifetime lifetime)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        Lifetime = lifetime;
    }

    public IEnumerable<Type> Dependencies =>
        Array.Empty<Type>();

    public ServiceLifetime Lifetime { get; }

    public object GetInstance(IDictionary<Type, object> parameters) =>
        _factory.Invoke() ?? throw FactoryReturnedNullException.For<TService>();
}