using System;
using System.Collections.Generic;
using FrankenBit.BoltWire.Exceptions;

namespace FrankenBit.BoltWire;

internal class FactoryRegistration<TImplementation> : IRegistration
{
    private readonly Func<IServiceProvider, TImplementation> _factory;

    private readonly IServiceProvider _resolver;

    public FactoryRegistration(IServiceProvider resolver,
        Func<IServiceProvider, TImplementation> factory, ServiceLifetime lifetime)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        Lifetime = lifetime;
    }

    public IEnumerable<Type> Dependencies =>
        Array.Empty<Type>();
        
    public ServiceLifetime Lifetime { get; }

    public object GetInstance(IDictionary<Type, object> parameters) =>
        _factory.Invoke(_resolver) ?? throw FactoryReturnedNullException.For<TImplementation>();
}