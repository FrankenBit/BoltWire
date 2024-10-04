using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FrankenBit.BoltWire;

public static class ExtensionsForIServiceCollection
{
    public static ServiceProvider Build(this IServiceCollection services)
    {
        var registry = new ServiceRegistry(new GreedyConstructorSelector());
        foreach (IServiceDescriptor descriptor in services) descriptor.Configure(registry);

        return new ServiceProvider(new CollectionCapableServiceRegistry(registry));
    }

    public static IPendingImplementation<TService> Register<TService>(this IServiceCollection services,
        ServiceLifetime lifetime, string? key = default) where TService : class
    {
        var descriptor = new ImplementationServiceDescriptor<TService>(lifetime, key);
        services.Add(descriptor);
        return new PendingImplementation<TService>(services, descriptor);
    }

    public static IPendingService<TService> Register<TService, TImplementation>(this IServiceCollection services,
        ServiceLifetime lifetime, string? key = default) where TImplementation : TService where TService : class
    {
        var descriptor = new ServiceDescriptor<TService, TImplementation>(lifetime, key);
        services.Add(descriptor);
        return new PendingService<TService>(services);
    }

    public static IServiceCollection Register(this IServiceCollection services,
        ServiceSetup<IServiceCollection> setup) =>
        setup.Invoke(services);

    public static IPendingImplementation<TService> RegisterComponentsInHierarchy<TService>(
        this IServiceCollection services, string? key = default) where TService : class
    {
        var descriptor = new ComponentsInHierarchyImplementationDescriptor<TService>(key);
        services.Add(descriptor);
        return new PendingImplementation<TService>(services, descriptor);
    }

    public static IPendingService<TService> RegisterComponentsInHierarchy<TService, TImplementation>(
        this IServiceCollection services, string? key = default) where TImplementation : TService where TService : class
    {
        var descriptor = new ComponentsInHierarchyDescriptor<TService, TImplementation>(key);
        services.Add(descriptor);
        return new PendingService<TService>(services);
    }

    public static IPendingImplementation<TService> RegisterScoped<TService>(
        this IServiceCollection services) where TService : class =>
        services.Register<TService>(ServiceLifetime.Scoped);

    public static IPendingService<TService> RegisterScoped<TService, TImplementation>(
        this IServiceCollection services) where TImplementation : TService where TService : class =>
        Register<TService, TImplementation>(services, ServiceLifetime.Scoped);

    private interface IImplementationServiceDescriptor : IServiceDescriptor
    {
        IServiceDescriptor AsImplementedInterfaces();
    }

    private sealed class ComponentsInHierarchyDescriptor<TService, TImplementation> : IServiceDescriptor
        where TImplementation : TService where TService : class
    {
        private readonly string? _key;

        internal ComponentsInHierarchyDescriptor(string? key) =>
            _key = key;

        public void Configure(IServiceRegistry registry)
        {
            IServiceRegistration<TService> registration = registry.GetRegistration<TService>(_key);

            foreach (TImplementation component in Object
                         .FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
                         .OfType<TImplementation>()) registration.Register(component);
        }
    }

    private sealed class ComponentsInHierarchyImplementationDescriptor<TService> : IImplementationServiceDescriptor
        where TService : class
    {
        private readonly string? _key;

        internal ComponentsInHierarchyImplementationDescriptor(string? key) =>
            _key = key;

        public void Configure(IServiceRegistry registry)
        {
            IServiceRegistration<TService> registration = registry.GetRegistration<TService>(_key);

            foreach (TService component in Object
                         .FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
                         .OfType<TService>()) registration.Register(component);
        }

        public IServiceDescriptor AsImplementedInterfaces() =>
            new ComponentsInHierarchyAsImplementedInterfacesDescriptor<TService>(_key);
    }

    private sealed class ComponentsInHierarchyAsImplementedInterfacesDescriptor<TService> : IServiceDescriptor
        where TService : class
    {
        private readonly string? _key;

        internal ComponentsInHierarchyAsImplementedInterfacesDescriptor(string? key) =>
            _key = key;

        public void Configure(IServiceRegistry registry)
        {
            List<IServiceRegistration> registrations = registry.GetInterfaceRegistrations<TService>(_key).ToList();

            foreach (TService component in Object
                         .FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
                         .OfType<TService>())
            foreach (IServiceRegistration registration in registrations)
                registration.Register(component);
        }
    }

    private sealed class ImplementationServiceDescriptor<TService> : IImplementationServiceDescriptor
        where TService : class
    {
        private readonly string? _key;

        private readonly ServiceLifetime _lifetime;

        internal ImplementationServiceDescriptor(ServiceLifetime lifetime, string? key)
        {
            _lifetime = lifetime;
            _key = key;
        }

        public void Configure(IServiceRegistry registry) =>
            registry.GetRegistration<TService>(_key).Register<TService>(_lifetime);

        public IServiceDescriptor AsImplementedInterfaces() =>
            new InterfaceServicesDescriptor<TService>(_lifetime, _key);
    }

    private sealed class InterfaceServicesDescriptor<TService> : IServiceDescriptor where TService : class
    {
        private readonly string? _key;

        private readonly ServiceLifetime _lifetime;

        public InterfaceServicesDescriptor(ServiceLifetime lifetime, string? key)
        {
            _lifetime = lifetime;
            _key = key;
        }

        public void Configure(IServiceRegistry registry)
        {
            foreach (IServiceRegistration registration in registry.GetInterfaceRegistrations<TService>(_key))
                registration.Register<TService>(_lifetime);
        }
    }

    private sealed class PendingImplementation<TService> : IPendingImplementation<TService>
    {
        private readonly IServiceCollection _services;

        private readonly IImplementationServiceDescriptor _descriptor;

        internal PendingImplementation(IServiceCollection services, IImplementationServiceDescriptor descriptor)
        {
            _services = services;
            _descriptor = descriptor;
        }

        public IPendingService<TService> AsImplementedInterfaces()
        {
            _services.Remove(_descriptor);
            IServiceDescriptor descriptor = _descriptor.AsImplementedInterfaces();
            _services.Add(descriptor);
            return new PendingService<TService>(_services);
        }

        public IEnumerator<IServiceDescriptor> GetEnumerator() =>
            _services.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)_services).GetEnumerator();

        public int Count =>
            _services.Count;

        public void Add(IServiceDescriptor descriptor) =>
            _services.Add(descriptor);

        public void Remove(IServiceDescriptor descriptor) =>
            _services.Remove(descriptor);
    }

    private class PendingService<TService> : IPendingService<TService>
    {
        private readonly IServiceCollection _services;

        internal PendingService(IServiceCollection services) =>
            _services = services;

        public int Count =>
            _services.Count;

        public IEnumerator<IServiceDescriptor> GetEnumerator() =>
            _services.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)_services).GetEnumerator();

        public void Add(IServiceDescriptor descriptor) =>
            _services.Add(descriptor);

        public void Remove(IServiceDescriptor descriptor) =>
            _services.Remove(descriptor);
    }

    private sealed class ServiceDescriptor<TService, TImplementation> : IServiceDescriptor
        where TService : class where TImplementation : TService
    {
        private readonly string? _key;

        private readonly ServiceLifetime _lifetime;

        internal ServiceDescriptor(ServiceLifetime lifetime, string? key)
        {
            _lifetime = lifetime;
            _key = key;
        }

        public void Configure(IServiceRegistry registry) =>
            registry.GetRegistration<TService>(_key).Register<TImplementation>(_lifetime);
    }
}
