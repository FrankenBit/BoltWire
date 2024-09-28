using System;

namespace FrankenBit.BoltWire
{
    internal interface IRegistry
    {
        void Decorate<TService, TDecorator>()
            where TDecorator : TService;
        
        void Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TImplementation : TService;

        void Register<TService>(TService singleton);

        void RegisterFactory<TService>(Func<TService> factory, ServiceLifetime lifetime = ServiceLifetime.Transient);

        void RegisterFactory<TImplementation>(Type serviceType, Func<IServiceProvider, TImplementation> factory,
            ServiceLifetime lifetime = ServiceLifetime.Transient);
    }
}
