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

internal sealed class CollectionCapableServiceRegistry : IServiceRegistry
{
    private readonly IServiceRegistry _registry;

    internal CollectionCapableServiceRegistry(IServiceRegistry registry) =>
        _registry = registry;

    public IServiceRegistration<TService> GetRegistration<TService>(string? key) where TService : class =>
        _registry.GetRegistration<TService>(key);

    public bool TryGetRegistration(Type serviceType, string? key,
        [NotNullWhen(true)] out IServiceRegistration? registration) =>
        _registry.TryGetRegistration(serviceType, key, out registration) ||
        TryGetCollectionRegistration(serviceType, key, out registration);

    private bool TryGetCollectionRegistration(Type serviceCollectionType, string? key, out IServiceRegistration? registration)
    {
        registration = default;
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
        _registry.TryGetRegistration(serviceType, key, out IServiceRegistration? registration)
            ? registration.Resolve(new ServiceContext(_registry, _disposables.Add))
            : default;
}

internal sealed class ServiceContext
{
    private readonly Action<IDisposable> _disposableStore;

    private readonly IServiceRegistry _registry;
    
    private readonly Stack<Type> _resolutionStack = new();

    internal ServiceContext(IServiceRegistry registry, Action<IDisposable> disposableStore)
    {
        _registry = registry;
        _disposableStore = disposableStore;
    }

    public object GetDependency(Type serviceType, Type dependencyType, string? key)
    {
        if (!_registry.TryGetRegistration(dependencyType, key, out IServiceRegistration? registration))
            throw new UnresolvedDependencyException(serviceType, dependencyType);

        if (_resolutionStack.TryPeek(out Type currentServiceType) && currentServiceType == dependencyType)
            return registration.Resolve(this);
        
        if (_resolutionStack.Contains(dependencyType))
            throw new CircularDependencyException(serviceType, dependencyType);
        
        _resolutionStack.Push(dependencyType);
        object result = registration.Resolve(this);
        _resolutionStack.Pop();
        return result;
    }

    public TService Track<TService>(TService service, ServiceLifetime lifetime) where TService : class
    {
        if (lifetime is ServiceLifetime.Scoped or ServiceLifetime.Singleton && service is IDisposable disposable)
            _disposableStore.Invoke(disposable);
        
        return service;
    }
}

internal static class SupportedCollectionTypes
{
    private const BindingFlags Binding = BindingFlags.Static | BindingFlags.NonPublic;
    
    internal static IReadOnlyCollection<Type> For<TService>() =>
        new[]
        {
            typeof(IEnumerable<TService>),
            typeof(IReadOnlyCollection<TService>),
            typeof(TService[])
        };

    internal static IReadOnlyCollection<Type> For(Type serviceType) =>
        (IReadOnlyCollection<Type>)GetGenericVariant(serviceType).Invoke(null, Array.Empty<object>());

    private static MethodInfo GetGenericVariant(Type serviceType) =>
        typeof(SupportedCollectionTypes).GetMethod(nameof(For), Binding, default, Type.EmptyTypes, default)!
            .MakeGenericMethod(serviceType);
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

        private readonly List<IServicePartRegistration> _decorators = new();

        private readonly List<IServicePartRegistration> _parts = new();

        private IServicePartRegistration? _composite;

        internal ServiceRegistration(IConstructorSelector constructorSelector) =>
            _constructorSelector = constructorSelector;

        public void Register(TService instance) =>
            _parts.Add(new SingletonRegistration(instance));

        public void Register<TImplementation>(ServiceLifetime lifetime) where TImplementation : TService =>
            Add(ImplementationRegistration.Create<TImplementation>(_constructorSelector, lifetime));

        public void Register(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime) =>
            Add(new FactoryRegistration(factory, lifetime));

        public object Resolve(ServiceContext context) =>
            Decorate(ResolveCompositeOrLast(context), context);

        IEnumerable IServiceRegistration.ResolveAll(ServiceContext context) =>
            ResolveAll(context);

        private static bool IsComposite(IServicePartRegistration registration) =>
            SupportedCollectionTypes.For<TService>().Any(registration.Dependencies.Contains);

        private static bool IsDecorator(IServicePartRegistration registration) =>
            registration.Dependencies.Contains(typeof(TService));

        private void Add(IServicePartRegistration registration)
        {
            if (registration.Lifetime == ServiceLifetime.Singleton)
                registration = new SingletonCacheRegistration(registration);
            
            if (IsDecorator(registration)) _decorators.Add(registration);
            else if (IsComposite(registration)) SetComposite(registration);
            else _parts.Add(registration);
        }

        private TService Decorate(TService service, ServiceContext context) =>
            _decorators.Aggregate(service, (current, decorator) => decorator.Resolve(context, current));

        private TService[] ResolveAll(ServiceContext context) =>
            _parts.Select(part => Decorate(part.Resolve(context), context)).ToArray();

        private TService ResolveCompositeOrLast(ServiceContext context) =>
            _composite?.Resolve(context, _parts.Select(part => part.Resolve(context))) ??
            _parts.Last().Resolve(context);

        private void SetComposite(IServicePartRegistration registration)
        {
            if (_composite is not null) throw CompositeAlreadySetException.For<TService>();

            _composite = registration;
        }

        private interface IServicePartRegistration
        {
            IEnumerable<Type> Dependencies { get; }
            
            ServiceLifetime Lifetime { get; }

            TService Resolve(ServiceContext context, params object[] dependencies) =>
                Resolve(context, (IEnumerable<object>)dependencies);

            TService Resolve(ServiceContext context, IEnumerable<object> dependencies);
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

            public TService Resolve(ServiceContext context, IEnumerable<object> dependencies) =>
                context.Track(Create(context), Lifetime);

            private TService Create(ServiceContext context) =>
                _factory.Invoke(new FactoryServiceProvider(context)) ??
                throw FactoryReturnedNullException.For<TService>();

            private sealed class FactoryServiceProvider : IServiceProvider
            {
                private readonly ServiceContext _context;

                internal FactoryServiceProvider(ServiceContext context) =>
                    _context = context;

                public object GetService(Type serviceType) =>
                    GetService(serviceType, default);

                public object GetService(Type serviceType, string? key) =>
                    _context.GetDependency(typeof(TService), serviceType, key);
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

            public TService Resolve(ServiceContext context, IEnumerable<object> dependencies) =>
                context.Track(Create(context, dependencies), Lifetime);

            private TService Create(ServiceContext context, IEnumerable<object> dependencies)
            {
                List<object> existingDependencies = dependencies.ToList();

                object[] allDependencies = _dependencies
                    .Except(existingDependencies.Select(dependency => dependency.GetType()))
                    .Select(type =>
                        context.GetDependency(typeof(TImplementation), type, default) ??
                        throw new UnresolvedDependencyException(typeof(TImplementation), type))
                    .Concat(existingDependencies)
                    .ToArray();

                return (TService)_constructor.Invoke(allDependencies);
            }
        }

        private static class ImplementationRegistration
        {
            internal static IServicePartRegistration Create<TImplementation>(IConstructorSelector constructorSelector,
                ServiceLifetime lifetime) where TImplementation : TService
            {
                if (IsTransientHidingDisposable<TImplementation>(lifetime))
                    throw HiddenDisposableRegistrationException.Of<TService, TImplementation>();
                
                ConstructorInfo constructor = constructorSelector.SelectConstructor<TImplementation>();
                Type[] dependencies = constructor.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .ToArray();
                return new ImplementationRegistration<TImplementation>(constructor, dependencies, lifetime);
            }

            private static bool IsTransientHidingDisposable<TImplementation>(ServiceLifetime lifetime)
                where TImplementation : TService =>
                lifetime == ServiceLifetime.Transient &&
                typeof(IDisposable).IsAssignableFrom(typeof(TImplementation)) &&
                !typeof(IDisposable).IsAssignableFrom(typeof(TService));
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

            public TService Resolve(ServiceContext context, IEnumerable<object> dependencies) =>
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

            public TService Resolve(ServiceContext context, IEnumerable<object> dependencies) =>
                _instance ??= _registration.Resolve(context, dependencies);
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
    object Resolve(ServiceContext context);

    IEnumerable ResolveAll(ServiceContext context);
}

internal interface IServiceRegistration<in TService> : IServiceRegistration where TService : class
{
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
