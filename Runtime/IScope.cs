using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using FrankenBit.BoltWire.Exceptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FrankenBit.BoltWire;

public interface IScope : IDisposable, IServiceProvider
{
    IScope CreateScope();

    [ContractAnnotation("=> true, instance: notnull; => false, instance: null")]
    bool TryGetExistingInstance(Type serviceType, out object instance); 
}

public delegate TServices ServiceSetup<TServices>(TServices services) where TServices : IServiceCollection;

public interface IServiceDescriptor
{
    internal void Configure(IServiceRegistry registry);
}

public interface IServiceCollection : IReadOnlyCollection<IServiceDescriptor>
{
    void Add(IServiceDescriptor descriptor);

    void Remove(IServiceDescriptor descriptor);
}

public sealed class ServiceCollection : IServiceCollection
{
    private readonly List<IServiceDescriptor> _descriptors = new();

    public int Count =>
        _descriptors.Count;

    public IEnumerator<IServiceDescriptor> GetEnumerator() =>
        _descriptors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void Add(IServiceDescriptor descriptor) =>
        _descriptors.Add(descriptor);

    public void Remove(IServiceDescriptor descriptor) =>
        _descriptors.Remove(descriptor);
}

internal interface IServiceRegistry
{
    IServiceRegistration<TService> GetRegistration<TService>(string? key);
    
    bool TryGetRegistration(Type serviceType, string? key, [NotNullWhen(true)] out IServiceRegistration? registration);
}

internal static class ExtensionsForIServiceRegistry
{
    internal static IEnumerable<IServiceRegistration> GetInterfaceRegistrations<TService>(
        this IServiceRegistry registry, string? key) =>
        registry.GetRegistrations<TService>(key, typeof(TService).GetInterfaces());

    internal static IServiceRegistration GetRegistration(this IServiceRegistry registry, Type serviceType,
        string? key) =>
        (IServiceRegistration)typeof(IServiceRegistry).GetMethod(nameof(GetRegistration))!
            .MakeGenericMethod(serviceType)
            .Invoke(registry, new object?[] { key });

    internal static object? GetService(this IServiceRegistry registry, Type serviceType, string? key) =>
        registry.TryGetRegistration(serviceType, key, out IServiceRegistration? registration)
            ? registration.Resolve(new ServiceContext(registry))
            : default;

    internal static object? GetService(this IServiceRegistry registry, ServiceContext context, Type serviceType,
        string? key) =>
        registry.TryGetRegistration(serviceType, key, out IServiceRegistration? registration)
            ? registration.Resolve(context)
            : default;
    
    private static IEnumerable<IServiceRegistration> GetRegistrations<TService>(this IServiceRegistry registry,
        string? key, IReadOnlyCollection<Type> interfaceTypes)
    {
        if (interfaceTypes.Count == 0) throw ServiceDoesNotImplementAnyInterfaces.For<TService>();

        return interfaceTypes.Select(interfaceType => registry.GetRegistration(interfaceType, key));
    }
}

public interface IPendingService<in TService> : IServiceCollection
{
    public IPendingService<TService> DecorateWith<TDecorator>() where TDecorator : TService =>
        throw new NotImplementedException();
}

public interface IPendingImplementation<in TService> :
    IPendingService<TService>
{
    public IPendingService<TService> AsImplementedInterfaces();
}

public interface IServiceProvider : System.IServiceProvider
{
    object? GetService(Type serviceType, string? key);
}

public sealed class ServiceProvider : IDisposable, IServiceProvider
{
    private readonly IServiceRegistry _registry;

    internal ServiceProvider(IServiceRegistry registry) =>
        _registry = registry;

    public void Dispose() =>
        Debug.LogWarning("Dispose not yet implemented");

    object? System.IServiceProvider.GetService(Type serviceType) =>
        GetService(serviceType);

    public object? GetService(Type serviceType, string? key = default) =>
        _registry.GetService(serviceType, key);
}

internal sealed class ServiceContext
{
    private readonly IServiceRegistry _registry;

    internal ServiceContext(IServiceRegistry registry) =>
        _registry = registry;

    public object? GetService(Type serviceType, string? key) =>
        _registry.GetService(this, serviceType, key);
}

internal sealed class ServiceRegistry : IServiceRegistry
{
    private readonly IConstructorSelector _constructorSelector;

    private readonly Dictionary<Type, IServiceGroupRegistration> _registrations = new();

    internal ServiceRegistry(IConstructorSelector constructorSelector) =>
        _constructorSelector = constructorSelector;
    
    public IServiceRegistration<TService> GetRegistration<TService>(string? key)
    {
        ServiceGroupRegistration<TService> typedRegistrations =
            _registrations.TryGetValue(typeof(TService), out IServiceGroupRegistration? descriptors)
                ? (ServiceGroupRegistration<TService>)descriptors
                : Store(new ServiceGroupRegistration<TService>(_constructorSelector));
        return typedRegistrations.GetRegistration(key);
    }

    public bool TryGetRegistration(Type serviceType, string? key,
        [NotNullWhen(true)] out IServiceRegistration? registration)
    {
        registration = default;
        
        return _registrations.TryGetValue(serviceType, out IServiceGroupRegistration registrations) &&
               registrations.TryGetRegistration(key, out registration);
    }

    private ServiceGroupRegistration<TService> Store<TService>(
        ServiceGroupRegistration<TService> serviceGroupRegistration)
    {
        _registrations[typeof(TService)] = serviceGroupRegistration;
        return serviceGroupRegistration;
    }

    private interface IServiceGroupRegistration
    {
        IServiceRegistration GetRegistration(string? key);
        
        bool TryGetRegistration(string? key, [NotNullWhen(true)] out IServiceRegistration? registration);
    }

    private sealed class ServiceRegistration<TService> : IServiceRegistration<TService>
    {
        private readonly IConstructorSelector _constructorSelector;

        private readonly List<IServicePartRegistration> _decorators = new();

        private readonly List<IServicePartRegistration> _parts = new();

        private IServicePartRegistration? _composite;

        internal ServiceRegistration(IConstructorSelector constructorSelector) =>
            _constructorSelector = constructorSelector;

        public void Decorate<TDecorator>() where TDecorator : TService
        {
            // maybe the decorators could also be registered using the regular Register<TImplementation> method?
            throw new NotImplementedException();
        }

        public void Register(TService instance) =>
            _parts.Add(new SingletonRegistration(instance));

        public void Register<TImplementation>(ServiceLifetime lifetime) where TImplementation : TService =>
            _parts.Add(new ImplementationRegistration<TImplementation>(_constructorSelector, lifetime));

        public void Register(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime) =>
            _parts.Add(new FactoryRegistration(factory, lifetime));

        public object Resolve(ServiceContext serviceContext) =>
            _parts.Last().Resolve(serviceContext);

        private interface IServicePartRegistration
        {
            object Resolve(ServiceContext context);
        }

        private sealed class FactoryRegistration : IServicePartRegistration
        {
            private readonly Func<IServiceProvider, TService> _factory;

            private readonly ServiceLifetime _lifetime;

            internal FactoryRegistration(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime)
            {
                _factory = factory;
                _lifetime = lifetime;
            }

            public object Resolve(ServiceContext context) =>
                _factory.Invoke(new FactoryServiceProvider(context)) ??
                throw FactoryReturnedNullException.For<TService>();

            private sealed class FactoryServiceProvider : IServiceProvider
            {
                private readonly ServiceContext _context;

                internal FactoryServiceProvider(ServiceContext context) =>
                    _context = context;

                public object? GetService(Type serviceType) =>
                    GetService(serviceType, default);

                public object? GetService(Type serviceType, string? key) =>
                    _context.GetService(serviceType, key);
            }
        }

        private sealed class ImplementationRegistration<TImplementation> : IServicePartRegistration
            where TImplementation : TService
        {
            private readonly IConstructorSelector _constructorSelector;

            private readonly ServiceLifetime _lifetime;

            internal ImplementationRegistration(IConstructorSelector constructorSelector, ServiceLifetime lifetime)
            {
                _constructorSelector = constructorSelector;
                _lifetime = lifetime;
            }

            public object Resolve(ServiceContext context)
            {
                ConstructorInfo constructor = _constructorSelector.SelectConstructor<TImplementation>();

                object[] dependencies = constructor.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .Select(type =>
                        context.GetService(type, default) ??
                        throw new UnresolvedDependencyException(typeof(TImplementation), type))
                    .ToArray();

                return constructor.Invoke(dependencies);
            }
        }

        private sealed class SingletonRegistration : IServicePartRegistration
        {
            private readonly TService _instance;

            internal SingletonRegistration(TService instance) =>
                _instance = instance;

            public object Resolve(ServiceContext context) =>
                _instance!;
        }
    }

    private sealed class ServiceGroupRegistration<TService> : IServiceGroupRegistration
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

internal interface IServiceRegistration
{
    object Resolve(ServiceContext serviceContext);
}

internal interface IServiceRegistration<in TService> : IServiceRegistration
{
    void Decorate<TDecorator>() where TDecorator : TService;

    void Register(TService instance);

    void Register<TImplementation>(ServiceLifetime lifetime) where TImplementation : TService;

    void Register(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime);
}

internal static class ExtensionsForIServiceRegistration
{
    internal static void Register<TService>(this IServiceRegistration registration, TService instance)
    {
        Type registrationServiceType = registration.GetType().GetGenericArguments().Single();
        registration.GetType()
            .GetMethod(nameof(IServiceRegistration<TService>.Register), new[] { registrationServiceType })!
            .Invoke(registration, new object?[] { instance });
    }

    internal static void Register<TService>(this IServiceRegistration registration, ServiceLifetime lifetime) =>
        registration.GetType()
            .GetMethod(nameof(IServiceRegistration<TService>.Register), new[] { typeof(ServiceLifetime) })!
            .Invoke(registration, new object?[] { lifetime });
}