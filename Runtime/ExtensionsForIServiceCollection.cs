using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace FrankenBit.BoltWire;

public static class ExtensionsForIServiceCollection
{
    public static ServiceProvider Build(this IServiceCollection services)
    {
        var constructorSelector = new GreedyConstructorSelector();
        var registry = new ServiceRegistry(constructorSelector);
        foreach (IServiceDescriptor descriptor in services) descriptor.Configure(registry);

        return new ServiceProvider(new CollectionCapableServiceRegistry(registry));
    }

    public static IPendingImplementation<TService> Register<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicConstructors)] TService>(
        this IServiceCollection services,
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
        return new PendingService<TService>(services, descriptor);
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

    public static IPendingService<TService> RegisterComponentsInHierarchy<TService,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicConstructors)] TImplementation>(
        this IServiceCollection services, string? key = default)
        where TService : class
        where TImplementation : MonoBehaviour, TService
    {
        var descriptor = new ComponentsInHierarchyDescriptor<TService, TImplementation>(key);
        services.Add(descriptor);
        return new PendingService<TService>(services, descriptor);
    }

    public static IPendingImplementation<TService> RegisterScoped<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicConstructors)] TService>(
        this IServiceCollection services) where TService : class =>
        services.Register<TService>(ServiceLifetime.Scoped);

    public static IPendingService<TService> RegisterScoped<TService,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicConstructors)] TImplementation>(
        this IServiceCollection services) where TImplementation : TService where TService : class =>
        Register<TService, TImplementation>(services, ServiceLifetime.Scoped);

    private interface IImplementationServiceDescriptor : IServiceDescriptor
    {
        IServiceDescriptor AsImplementedInterfaces();
    }

    private sealed class ComponentsInHierarchyDescriptor<TService, TImplementation> : IServiceDescriptor
        where TImplementation : TService where TService : class
    {
        internal ComponentsInHierarchyDescriptor(string? key) =>
            Key = key;

        public string? Key { get; }

        public ServiceLifetime Lifetime =>
            ServiceLifetime.Singleton;

        public void Configure(IServiceRegistry registry)
        {
            IServiceRegistration<TService> registration = registry.GetRegistration<TService>(Key);

            foreach (TImplementation component in Object
                         .FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
                         .OfType<TImplementation>()) registration.Register(component);
        }
    }

    private sealed class ComponentsInHierarchyImplementationDescriptor<TService> : IImplementationServiceDescriptor
        where TService : class
    {
        internal ComponentsInHierarchyImplementationDescriptor(string? key) =>
            Key = key;

        public string? Key { get; }

        public ServiceLifetime Lifetime =>
            ServiceLifetime.Singleton;

        public void Configure(IServiceRegistry registry)
        {
            IServiceRegistration<TService> registration = registry.GetRegistration<TService>(Key);

            foreach (TService component in Object
                         .FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
                         .OfType<TService>()) registration.Register(component);
        }

        public IServiceDescriptor AsImplementedInterfaces() =>
            new ComponentsInHierarchyAsImplementedInterfacesDescriptor<TService>(Key);
    }

    private sealed class ComponentsInHierarchyAsImplementedInterfacesDescriptor<TService> : IServiceDescriptor
        where TService : class
    {
        internal ComponentsInHierarchyAsImplementedInterfacesDescriptor(string? key) =>
            Key = key;

        public string? Key { get; }

        public ServiceLifetime Lifetime =>
            ServiceLifetime.Singleton;

        public void Configure(IServiceRegistry registry)
        {
            List<IServiceRegistration> registrations = registry.GetInterfaceRegistrations<TService>(Key).ToList();

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
        internal ImplementationServiceDescriptor(ServiceLifetime lifetime, string? key)
        {
            Lifetime = lifetime;
            Key = key;
        }

        public void Configure(IServiceRegistry registry) =>
            registry.GetRegistration<TService>(Key).Register<TService>(Lifetime);

        public string? Key { get; }

        public ServiceLifetime Lifetime { get; }

        public IServiceDescriptor AsImplementedInterfaces() =>
            new InterfaceServicesDescriptor<TService>(Lifetime, Key);
    }

    private sealed class InterfaceServicesDescriptor<TService> : IServiceDescriptor where TService : class
    {
        internal InterfaceServicesDescriptor(ServiceLifetime lifetime, string? key)
        {
            Lifetime = lifetime;
            Key = key;
        }

        public string? Key { get; }

        public ServiceLifetime Lifetime { get; }

        public void Configure(IServiceRegistry registry)
        {
            IServicePartRegistration<TService> partRegistration =
                ImplementationRegistration.Create<TService, TService>(registry.ConstructorSelector, Lifetime)
                    .CacheIfNeeded();

            foreach (IServiceRegistration registration in registry.GetInterfaceRegistrations<TService>(Key))
                registration.Add(partRegistration);
        }
    }

    private sealed class PendingImplementation<TService> : IPendingImplementation<TService> where TService : class
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
            return new PendingService<TService>(_services, _descriptor);
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

        public string? Key =>
            _descriptor.Key;

        public ServiceLifetime Lifetime =>
            _descriptor.Lifetime;
    }

    private class PendingService<TService> : IPendingService<TService> where TService : class
    {
        private readonly IServiceDescriptor _descriptor;

        private readonly IServiceCollection _services;

        internal PendingService(IServiceCollection services, IServiceDescriptor descriptor)
        {
            _services = services;
            _descriptor = descriptor;
        }

        public int Count =>
            _services.Count;

        public string? Key =>
            _descriptor.Key;

        public ServiceLifetime Lifetime =>
            _descriptor.Lifetime;

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
        internal ServiceDescriptor(ServiceLifetime lifetime, string? key)
        {
            Lifetime = lifetime;
            Key = key;
        }

        public string? Key { get; }

        public ServiceLifetime Lifetime { get; }

        public void Configure(IServiceRegistry registry) =>
            registry.GetRegistration<TService>(Key).Register<TImplementation>(Lifetime);
    }
}
