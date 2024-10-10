using System;

namespace FrankenBit.BoltWire;

internal interface IServiceRegistration<in TService> : IServiceRegistration where TService : class
{
    void Add(IServicePartRegistration<TService> registration);
    
    void Register(TService instance);

    void Register<TImplementation>(ServiceLifetime lifetime) where TImplementation : TService;

    void Register(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime);
}
