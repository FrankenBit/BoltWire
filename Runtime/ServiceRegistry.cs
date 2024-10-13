using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using FrankenBit.BoltWire.Exceptions;

namespace FrankenBit.BoltWire;

internal sealed class ServiceRegistry : IServiceRegistry
{
    private readonly IConstructorSelector _constructorSelector;

    private readonly Dictionary<Type, IServiceGroupRegistration> _registrations = new();

    internal ServiceRegistry(IConstructorSelector constructorSelector) =>
        _constructorSelector = constructorSelector;

    public IServiceRegistration<TService> GetRegistration<TService>(string? key) where TService : class
    {
        ServiceGroupRegistration<TService> typedRegistrations =
            _registrations.TryGetValue(typeof(TService), out IServiceGroupRegistration? descriptors)
                ? (ServiceGroupRegistration<TService>)descriptors
                : Store(new ServiceGroupRegistration<TService>(_constructorSelector));
        return typedRegistrations.GetRegistration(key);
    }

    public ConstructorInfo SelectConstructor(Type implementationType) =>
        _constructorSelector.SelectConstructor(implementationType);

    public bool TryGetRegistration(Type serviceType, string? key,
        [NotNullWhen(true)] out IServiceRegistration? registration)
    {
        registration = default;
        
        return _registrations.TryGetValue(serviceType, out IServiceGroupRegistration registrations) &&
               registrations.TryGetRegistration(key, out registration);
    }

    private ServiceGroupRegistration<TService> Store<TService>(
        ServiceGroupRegistration<TService> registration) where TService : class
    {
        _registrations[typeof(TService)] = registration;
        return registration;
    }

    private interface IServiceGroupRegistration
    {
        IServiceRegistration GetRegistration(string? key);
        
        bool TryGetRegistration(string? key, [NotNullWhen(true)] out IServiceRegistration? registration);
    }

    private sealed class ServiceRegistration<TService> : IServiceRegistration<TService> where TService : class
    {
        private readonly IConstructorSelector _constructorSelector;

        private readonly List<IServicePartRegistration<TService>> _decorators = new();

        private readonly List<IServicePartRegistration<TService>> _parts = new();

        private IServicePartRegistration<TService>? _composite;

        internal ServiceRegistration(IConstructorSelector constructorSelector) =>
            _constructorSelector = constructorSelector;

        public void Add(IServicePartRegistration<TService> registration) =>
            AddCached(registration.CacheIfNeeded());

        private void AddCached(IServicePartRegistration<TService> registration)
        {
            if (IsDecorator(registration)) _decorators.Add(registration);
            else if (IsComposite(registration)) SetComposite(registration);
            else _parts.Add(registration);
        }

        public void Register(TService instance) =>
            _parts.Add(new SingletonRegistration<TService>(instance));

        public void Register<TImplementation>(ServiceLifetime lifetime) where TImplementation : TService =>
            Add(ImplementationRegistration.Create<TService, TImplementation>(
            _constructorSelector.SelectConstructor(typeof(TImplementation)), lifetime));

        public void Register(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime) =>
            Add(new FactoryRegistration<TService>(factory, lifetime));

        public object Resolve(ServiceContext context) =>
            Decorate(ResolveCompositeOrLast(context), context);

        IEnumerable IServiceRegistration.ResolveAll(ServiceContext context) =>
            ResolveAll(context);

        private static bool IsComposite(IServicePartRegistration<TService> registration) =>
            CompositeType.IsComposite<TService>(registration.Dependencies);

        private static bool IsDecorator(IServicePartRegistration<TService> registration) =>
            registration.Dependencies.Contains(typeof(TService));

        private TService Decorate(TService service, ServiceContext context) =>
            _decorators.Aggregate(service, (current, decorator) => decorator.Resolve(context, current));

        private TService[] ResolveAll(ServiceContext context) =>
            _parts.Select(part => Decorate(part.Resolve(context), context)).ToArray();

        private TService ResolveCompositeOrLast(ServiceContext context) =>
            _composite?.Resolve(context, new object[] { _parts.Select(part => part.Resolve(context)).ToArray() }) ??
            _parts.Last().Resolve(context);

        private void SetComposite(IServicePartRegistration<TService> registration)
        {
            if (_composite is not null) throw CompositeAlreadySetException.For<TService>();

            _composite = registration;
        }
    }

    private sealed class ServiceGroupRegistration<TService> : IServiceGroupRegistration where TService : class
    {
        private readonly IConstructorSelector _constructorSelector;

        private readonly Dictionary<string, ServiceRegistration<TService>> _keyed = new();

        private readonly ServiceRegistration<TService> _notKeyed;

        internal ServiceGroupRegistration(IConstructorSelector constructorSelector)
        {
            _constructorSelector = constructorSelector;
            _notKeyed = new ServiceRegistration<TService>(constructorSelector);
        }
        
        IServiceRegistration IServiceGroupRegistration.GetRegistration(string? key) =>
            GetRegistration(key);

        public bool TryGetRegistration(string? key, [NotNullWhen(true)] out IServiceRegistration? registration)
        {
            if (key is null)
            {
                registration = _notKeyed;
                return true;
            }
            
            registration = default;
            if (!_keyed.TryGetValue(key, out ServiceRegistration<TService> serviceRegistration)) return false;
            
            registration = serviceRegistration;
            return true;
        }

        internal ServiceRegistration<TService> GetRegistration(string? key) =>
            key is null ? _notKeyed : GetKeyedRegistration(key);

        private ServiceRegistration<TService> GetKeyedRegistration(string key) =>
            _keyed.TryGetValue(key, out ServiceRegistration<TService> registration)
                ? registration
                : _keyed[key] = new ServiceRegistration<TService>(_constructorSelector);
    }
}
