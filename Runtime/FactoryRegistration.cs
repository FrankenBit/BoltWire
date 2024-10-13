using System;
using System.Collections.Generic;
using FrankenBit.BoltWire.Exceptions;

namespace FrankenBit.BoltWire;

internal sealed class FactoryRegistration<TService> : IServicePartRegistration<TService> where TService : class
{
    private readonly Func<IServiceProvider, TService> _factory;

    internal FactoryRegistration(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime)
    {
        _factory = factory;
        Lifetime = lifetime;
    }

    public IEnumerable<Type> Dependencies =>
        Array.Empty<Type>();

    public Type ImplementationType =>
        typeof(TService);

    public bool IsCaching =>
        false;

    public ServiceLifetime Lifetime { get; }

    public TService Resolve(IServiceContext context, IReadOnlyCollection<object> dependencies) =>
        context.Track(Create(context), Lifetime);

    private TService Create(IServiceContext context) =>
        _factory.Invoke(new FactoryServiceProvider(context)) ??
        throw FactoryReturnedNullException.For<TService>();

    private sealed class FactoryServiceProvider : IServiceProvider
    {
        private readonly IServiceContext _context;

        internal FactoryServiceProvider(IServiceContext context) =>
            _context = context;

        public object GetService(Type serviceType) =>
            GetService(serviceType, default);

        public object GetService(Type serviceType, string? key) =>
            _context.GetDependency(typeof(TService), serviceType, key);
    }
}
