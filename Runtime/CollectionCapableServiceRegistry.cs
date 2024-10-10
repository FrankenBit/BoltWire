using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FrankenBit.BoltWire;

internal sealed class CollectionCapableServiceRegistry : IServiceRegistry
{
    private readonly IServiceRegistry _registry;

    internal CollectionCapableServiceRegistry(IServiceRegistry registry) =>
        _registry = registry;

    public IConstructorSelector ConstructorSelector =>
        _registry.ConstructorSelector;

    public IServiceRegistration<TService> GetRegistration<TService>(string? key) where TService : class =>
        _registry.GetRegistration<TService>(key);

    public bool TryGetRegistration(Type serviceType, string? key,
        [NotNullWhen(true)] out IServiceRegistration? registration) =>
        _registry.TryGetRegistration(serviceType, key, out registration) ||
        TryGetCollectionRegistration(serviceType, key, out registration);

    private bool TryGetCollectionRegistration(Type serviceCollectionType, string? key, out IServiceRegistration? registration)
    {
        registration = default;
        if (serviceCollectionType.GenericTypeArguments.Length != 1) return false;

        Type serviceType = serviceCollectionType.GenericTypeArguments.Single();
        if (!SupportedCollectionTypes.For(serviceType).Contains(serviceCollectionType)) return false;

        if (!_registry.TryGetRegistration(serviceType, key, out IServiceRegistration? innerRegistration))
            return false;

        registration = new ResolveAllRegistration(innerRegistration);
        return true;
    }

    private sealed class ResolveAllRegistration : IServiceRegistration
    {
        private readonly IServiceRegistration _registration;

        internal ResolveAllRegistration(IServiceRegistration registration) =>
            _registration = registration;

        public object Resolve(ServiceContext context) =>
            _registration.ResolveAll(context);

        public IEnumerable ResolveAll(ServiceContext context) =>
            new[] { Resolve(context) };
    }
}