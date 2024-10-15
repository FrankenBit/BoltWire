using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace FrankenBit.BoltWire;

public static partial class ExtensionsForIServiceCollection
{
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

    public static IPendingImplementation<TService> RegisterComponentsInHierarchy<TService>(
        this IServiceCollection services, string? key = default) where TService : class
    {
        var descriptor = new ComponentsInHierarchyImplementationDescriptor<TService>(key);
        services.Add(descriptor);
        return new PendingImplementation<TService>(services, descriptor);
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
}
