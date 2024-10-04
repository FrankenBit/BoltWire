using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using FrankenBit.BoltWire.Exceptions;
using FrankenBit.BoltWire.Tools;
using JetBrains.Annotations;

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
    IServiceRegistration<TService> GetRegistration<TService>(string? key) where TService : class;
    
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
    private readonly CompositeDisposable _disposables = new();
    
    private readonly IServiceRegistry _registry;

    internal ServiceProvider(IServiceRegistry registry) =>
        _registry = registry;

    public void Dispose() =>
        _disposables.Dispose();

    object? System.IServiceProvider.GetService(Type serviceType) =>
        GetService(serviceType);

    public object? GetService(Type serviceType, string? key = default) =>
        _registry.GetService(new ServiceContext(_registry, _disposables.Add), serviceType, key);
}

internal sealed class ServiceContext
{
    private readonly Action<IDisposable> _disposableStore;

    private readonly IServiceRegistry _registry;

    internal ServiceContext(IServiceRegistry registry, Action<IDisposable> disposableStore)
    {
        _registry = registry;
        _disposableStore = disposableStore;
    }

    public object? GetService(Type serviceType, string? key) =>
        _registry.GetService(this, serviceType, key);

    public TService Track<TService>(TService service, ServiceLifetime lifetime) where TService : class
    {
        if (lifetime is ServiceLifetime.Scoped or ServiceLifetime.Singleton && service is IDisposable disposable)
            _disposableStore.Invoke(disposable);
        
        return service;
    }
}

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

    public bool TryGetRegistration(Type serviceType, string? key,
        [NotNullWhen(true)] out IServiceRegistration? registration)
    {
        registration = default;
        
        return _registrations.TryGetValue(serviceType, out IServiceGroupRegistration registrations) &&
               registrations.TryGetRegistration(key, out registration);
    }

    private ServiceGroupRegistration<TService> Store<TService>(
        ServiceGroupRegistration<TService> serviceGroupRegistration) where TService : class
    {
        _registrations[typeof(TService)] = serviceGroupRegistration;
        return serviceGroupRegistration;
    }

    private interface IServiceGroupRegistration
    {
        IServiceRegistration GetRegistration(string? key);
        
        bool TryGetRegistration(string? key, [NotNullWhen(true)] out IServiceRegistration? registration);
    }

    private sealed class ServiceRegistration<TService> : IServiceRegistration<TService> where TService : class
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
            Add(ImplementationRegistration.Create<TImplementation>(_constructorSelector, lifetime));

        public void Register(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime) =>
            Add(new FactoryRegistration(factory, lifetime));

        public object Resolve(ServiceContext serviceContext) =>
            _parts.Last().Resolve(serviceContext);

        private void Add(IServicePartRegistration registration) =>
            _parts.Add(registration.Lifetime == ServiceLifetime.Singleton
                ? new SingletonCacheRegistration(registration)
                : registration);

        private interface IServicePartRegistration
        {
            IEnumerable<Type> Dependencies { get; }
            
            ServiceLifetime Lifetime { get; }
            
            object Resolve(ServiceContext context);
        }

        private sealed class FactoryRegistration : IServicePartRegistration
        {
            private readonly Func<IServiceProvider, TService> _factory;

            internal FactoryRegistration(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime)
            {
                _factory = factory;
                Lifetime = lifetime;
            }

            public IEnumerable<Type> Dependencies =>
                Array.Empty<Type>();
            
            public ServiceLifetime Lifetime { get; }

            public object Resolve(ServiceContext context) =>
                context.Track(Create(context), Lifetime);

            private TService Create(ServiceContext context) =>
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
            private readonly ConstructorInfo _constructor;

            private readonly Type[] _dependencies;

            internal ImplementationRegistration(ConstructorInfo constructor, Type[] dependencies,
                ServiceLifetime lifetime)
            {
                _constructor = constructor;
                _dependencies = dependencies;
                Lifetime = lifetime;
            }

            public IEnumerable<Type> Dependencies =>
                _dependencies;
            
            public ServiceLifetime Lifetime { get; }

            public object Resolve(ServiceContext context) =>
                context.Track(Create(context), Lifetime);

            private TService Create(ServiceContext context)
            {
                object[] dependencies = _dependencies
                    .Select(type =>
                        context.GetService(type, default) ??
                        throw new UnresolvedDependencyException(typeof(TImplementation), type))
                    .ToArray();

                return (TService)_constructor.Invoke(dependencies);
            }
        }

        private static class ImplementationRegistration
        {
            internal static IServicePartRegistration Create<TImplementation>(IConstructorSelector constructorSelector,
                ServiceLifetime lifetime) where TImplementation : TService
            {
                ConstructorInfo constructor = constructorSelector.SelectConstructor<TImplementation>();
                Type[] dependencies = constructor.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .ToArray();
                return new ImplementationRegistration<TImplementation>(constructor, dependencies, lifetime);
            }
        }

        private sealed class SingletonRegistration : IServicePartRegistration
        {
            private readonly TService _instance;

            internal SingletonRegistration(TService instance) =>
                _instance = instance;

            public IEnumerable<Type> Dependencies =>
                Array.Empty<Type>();

            public ServiceLifetime Lifetime =>
                ServiceLifetime.Singleton;

            public object Resolve(ServiceContext context) =>
                _instance;
        }

        private sealed class SingletonCacheRegistration : IServicePartRegistration
        {
            private readonly IServicePartRegistration _registration;

            private TService? _instance;

            internal SingletonCacheRegistration(IServicePartRegistration registration) =>
                _registration = registration;

            public IEnumerable<Type> Dependencies =>
                _instance is null ? _registration.Dependencies : Array.Empty<Type>();

            public ServiceLifetime Lifetime =>
                ServiceLifetime.Singleton;

            public object Resolve(ServiceContext context) =>
                _instance ??= (TService)_registration.Resolve(context);
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

internal interface IServiceRegistration
{
    object Resolve(ServiceContext serviceContext);
}

internal interface IServiceRegistration<in TService> : IServiceRegistration where TService : class
{
    void Decorate<TDecorator>() where TDecorator : TService;

    void Register(TService instance);

    void Register<TImplementation>(ServiceLifetime lifetime) where TImplementation : TService;

    void Register(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime);
}

internal static class ExtensionsForIServiceRegistration
{
    internal static void Register<TService>(this IServiceRegistration registration, TService instance)
        where TService : class
    {
        Type registrationServiceType = registration.GetType().GetGenericArguments().Single();
        registration.GetType()
            .GetMethod(nameof(IServiceRegistration<TService>.Register), new[] { registrationServiceType })!
            .Invoke(registration, new object?[] { instance });
    }

    internal static void Register<TService>(this IServiceRegistration registration, ServiceLifetime lifetime)
        where TService : class =>
        registration.GetType()
            .GetMethod(nameof(IServiceRegistration<TService>.Register), new[] { typeof(ServiceLifetime) })!
            .Invoke(registration, new object?[] { lifetime });
}
