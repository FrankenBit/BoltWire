using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using FrankenBit.BoltWire.Exceptions;
using FrankenBit.BoltWire.Tools;

namespace FrankenBit.BoltWire;

public interface IScope : IDisposable, IServiceProvider
{
    IScope CreateScope();

    bool TryGetService(Type serviceType, string? key, [NotNullWhen(true)] out object? instance);
}

public delegate TServices ServiceSetup<TServices>(TServices services) where TServices : IServiceCollection;

public interface IServiceDescriptor
{
    internal void Configure(IServiceRegistry registry);
    string? Key { get; }
    ServiceLifetime Lifetime { get; }
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
    IConstructorSelector ConstructorSelector { get; }
    
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

public interface IPendingService<in TService> : IServiceCollection where TService : class
{
    public IPendingService<TService> DecorateWith<TDecorator>() where TDecorator : TService
    {
        this.Register<TService, TDecorator>(Lifetime, Key);
        return this;
    }

    string? Key { get; }

    ServiceLifetime Lifetime { get; }
}

public interface IPendingImplementation<in TService> :
    IPendingService<TService> where TService : class
{
    public IPendingService<TService> AsImplementedInterfaces();
}

public interface IServiceProvider : System.IServiceProvider
{
    object? System.IServiceProvider.GetService(Type serviceType) =>
        GetService(serviceType, default);
    
    object? GetService(Type serviceType, string? key);
}

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

internal interface IInstanceTracker
{
    void Track(object service, ServiceLifetime lifetime, string? key = default)
    {
        if (service is IDisposable disposable) TrackDisposable(disposable, lifetime);
        TrackInstance(service, lifetime, key);
    }

    void TrackDisposable(IDisposable service, ServiceLifetime lifetime);

    void TrackInstance(object service, ServiceLifetime lifetime, string? key);
    
    bool TryGetService(Type serviceType, string? key, [NotNullWhen(true)] out object? instance);
}

public sealed class ServiceProvider : IScope, IInstanceTracker
{
    private readonly CompositeDisposable _disposables = new();

    private readonly Dictionary<InstanceKey, object> _instances = new();
    
    private readonly IServiceRegistry _registry;

    internal ServiceProvider(IServiceRegistry registry) =>
        _registry = registry;

    public IScope CreateScope() =>
        new ServiceScope(this, _registry, this);

    public void Dispose() =>
        _disposables.Dispose();

    public object? GetService(Type serviceType, string? key = default) =>
        TryGetService(serviceType, key, out object? instance)
            ? instance
            : _registry.TryGetRegistration(serviceType, key, out IServiceRegistration? registration)
                ? registration.Resolve(new ServiceContext(_registry, this, key))
                : default;

    void IInstanceTracker.TrackDisposable(IDisposable service, ServiceLifetime lifetime) =>
        _disposables.Add(service);

    void IInstanceTracker.TrackInstance(object service, ServiceLifetime lifetime, string? key)
    {
        if (lifetime is ServiceLifetime.Singleton or ServiceLifetime.Scoped)
            _instances[new InstanceKey(service.GetType(), key)] = service;
    }

    public bool TryGetService(Type serviceType, string? key, [NotNullWhen(true)] out object? instance) =>
        _instances.TryGetValue(new InstanceKey(serviceType, key), out instance);
}

internal sealed class ServiceScope : IScope, IInstanceTracker
{
    private readonly CompositeDisposable _disposables = new();

    private readonly Dictionary<InstanceKey, object> _instances = new();

    private readonly IScope _parent;

    private readonly IServiceRegistry _registry;

    private readonly IInstanceTracker _root;

    internal ServiceScope(IScope parent, IServiceRegistry registry, IInstanceTracker root)
    {
        _parent = parent;
        _registry = registry;
        _root = root;
    }

    public void Dispose() =>
        _disposables.Dispose();

    public object? GetService(Type serviceType, string? key) =>
        TryGetService(serviceType, key, out object? instance)
            ? instance
            : _registry.TryGetRegistration(serviceType, key, out IServiceRegistration? registration)
                ? registration.Resolve(new ServiceContext(_registry, this, key))
                : default;

    public IScope CreateScope() =>
        new ServiceScope(this, _registry, _root);

    void IInstanceTracker.TrackDisposable(IDisposable service, ServiceLifetime lifetime)
    {
        switch (lifetime)
        {
        case ServiceLifetime.Singleton:
            _root.TrackDisposable(service, lifetime);
            break;

        case ServiceLifetime.Scoped:
            _disposables.Add(service);
            break;

        case ServiceLifetime.Transient:
            break;

        default:
            throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
    }

    void IInstanceTracker.TrackInstance(object service, ServiceLifetime lifetime, string? key)
    {
        switch (lifetime)
        {
        case ServiceLifetime.Singleton:
            _root.TrackInstance(service, lifetime, key);
            break;

        case ServiceLifetime.Scoped:
            _instances[new InstanceKey(service.GetType(), key)] = service;
            break;

        case ServiceLifetime.Transient:
            break;

        default:
            throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
    }

    public bool TryGetService(Type serviceType, string? key, [NotNullWhen(true)] out object? instance) =>
        _instances.TryGetValue(new InstanceKey(serviceType, key), out instance) ||
        _parent.TryGetService(serviceType, key, out instance);
}

internal sealed record InstanceKey(Type Type, string? Key);

internal sealed class ServiceContext
{
    private readonly IInstanceTracker _tracker;

    private readonly IServiceRegistry _registry;
    
    private readonly Stack<Type> _resolutionStack = new();

    internal ServiceContext(IServiceRegistry registry, IInstanceTracker tracker, string? key)
    {
        Key = key;
        _registry = registry;
        _tracker = tracker;
    }

    internal string? Key { get; }

    internal object GetDependency(Type serviceType, Type dependencyType, string? key)
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

    internal TService Track<TService>(TService service, ServiceLifetime lifetime) where TService : class
    {
        _tracker.Track(service, lifetime, Key);
        return service;
    }

    public bool TryGetInstance(Type implementationType, [NotNullWhen(true)] out object? instance) =>
        _tracker.TryGetService(implementationType, Key, out instance);
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
    private readonly Dictionary<Type, IServiceGroupRegistration> _registrations = new();

    internal ServiceRegistry(IConstructorSelector constructorSelector) =>
        ConstructorSelector = constructorSelector;

    public IConstructorSelector ConstructorSelector { get; }

    public IServiceRegistration<TService> GetRegistration<TService>(string? key) where TService : class
    {
        ServiceGroupRegistration<TService> typedRegistrations =
            _registrations.TryGetValue(typeof(TService), out IServiceGroupRegistration? descriptors)
                ? (ServiceGroupRegistration<TService>)descriptors
                : Store(new ServiceGroupRegistration<TService>(ConstructorSelector));
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
            Add(ImplementationRegistration.Create<TService, TImplementation>(_constructorSelector, lifetime));

        public void Register(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime) =>
            Add(new FactoryRegistration<TService>(factory, lifetime));

        public object Resolve(ServiceContext context) =>
            Decorate(ResolveCompositeOrLast(context), context);

        IEnumerable IServiceRegistration.ResolveAll(ServiceContext context) =>
            ResolveAll(context);

        private static bool IsComposite(IServicePartRegistration<TService> registration) =>
            SupportedCollectionTypes.For<TService>().Any(registration.Dependencies.Contains);

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

    public TService Resolve(ServiceContext context, IReadOnlyCollection<object> dependencies) =>
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

internal sealed class SingletonRegistration<TService> : IServicePartRegistration<TService> where TService : class
{
    private readonly TService _instance;

    internal SingletonRegistration(TService instance) =>
        _instance = instance;

    public IEnumerable<Type> Dependencies =>
        Array.Empty<Type>();

    public Type ImplementationType=> 
        typeof(TService);

    public bool IsCaching =>
        true;

    public ServiceLifetime Lifetime =>
        ServiceLifetime.Singleton;

    public TService Resolve(ServiceContext context, IReadOnlyCollection<object> dependencies) =>
        _instance;
}

internal static class ExtensionsForIServicePartRegistration
{
    internal static IServicePartRegistration<TService> CacheIfNeeded<TService>(
        this IServicePartRegistration<TService> registration) where TService : class =>
        registration.IsCaching ? registration : Cache(registration);

    private static IServicePartRegistration<TService> Cache<TService>(IServicePartRegistration<TService> registration)
        where TService : class =>
        registration.Lifetime switch
        {
            ServiceLifetime.Singleton => new SingletonCacheRegistration<TService>(registration),
            ServiceLifetime.Scoped => new ScopedCacheRegistration<TService>(registration),
            _ => registration
        };
}

internal abstract class CacheRegistrationBase<TService> : IServicePartRegistration<TService> where TService : class
{
    private readonly IServicePartRegistration<TService> _registration;

    protected CacheRegistrationBase(IServicePartRegistration<TService> registration) =>
        _registration = registration;

    public virtual IEnumerable<Type> Dependencies =>
        _registration.Dependencies;

    public Type ImplementationType =>
        _registration.ImplementationType;

    public bool IsCaching =>
        true;

    public ServiceLifetime Lifetime =>
        _registration.Lifetime;

    public TService Resolve(ServiceContext context, IReadOnlyCollection<object> dependencies) =>
        GetCachedInstance(context) ?? _registration.Resolve(context, dependencies);
    
    protected virtual TService? GetCachedInstance(ServiceContext context) =>
        context.TryGetInstance(ImplementationType, out object? instance)
            ? (TService)instance
            : default; 
}

internal sealed class ScopedCacheRegistration<TService> : CacheRegistrationBase<TService> where TService : class
{
    internal ScopedCacheRegistration(IServicePartRegistration<TService> registration)
        : base(registration)
    {
    }
}

internal sealed class SingletonCacheRegistration<TService> : CacheRegistrationBase<TService> where TService : class
{
    private TService? _instance;

    internal SingletonCacheRegistration(IServicePartRegistration<TService> registration)
        : base(registration)
    {
    }

    public override IEnumerable<Type> Dependencies =>
        _instance is null ? base.Dependencies : Array.Empty<Type>();

    protected override TService? GetCachedInstance(ServiceContext context) =>
        _instance ?? base.GetCachedInstance(context);
}

internal sealed class ImplementationRegistration<TService, TImplementation> : IServicePartRegistration<TService>
    where TService : class where TImplementation : TService
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

    public Type ImplementationType =>
        typeof(TImplementation);

    public bool IsCaching =>
        false;

    public ServiceLifetime Lifetime { get; }

    public TService Resolve(ServiceContext context, IReadOnlyCollection<object> dependencies) =>
        context.Track(Create(context, dependencies), Lifetime);

    private TService Create(ServiceContext context, IReadOnlyCollection<object> dependencies)
    {
        List<Type> suppliedTypes = dependencies.Select(dependency => dependency.GetType()).ToList();

        object[] allDependencies = _dependencies
            .Where(dependency => !suppliedTypes.Any(dependency.IsAssignableFrom))
            .Select(type =>
                context.GetDependency(typeof(TImplementation), type, context.Key) ??
                throw new UnresolvedDependencyException(typeof(TImplementation), type))
            .Concat(dependencies.Where(dependency =>
                _dependencies.Any(desired => desired.IsAssignableFrom(dependency.GetType()))))
            .ToArray();

        return (TService)_constructor.Invoke(allDependencies);
    }
}

internal static class ImplementationRegistration
{
    internal static IServicePartRegistration<TService> Create<TService, TImplementation>(
        IConstructorSelector constructorSelector, ServiceLifetime lifetime)
        where TService : class
        where TImplementation : TService
    {
        if (IsTransientHidingDisposable<TService, TImplementation>(lifetime))
            throw HiddenDisposableRegistrationException.Of<TService, TImplementation>();

        ConstructorInfo constructor = constructorSelector.SelectConstructor<TImplementation>();
        Type[] dependencies = constructor.GetParameters()
            .Select(parameter => parameter.ParameterType)
            .ToArray();
        return new ImplementationRegistration<TService, TImplementation>(constructor, dependencies, lifetime);
    }

    private static bool IsTransientHidingDisposable<TService, TImplementation>(ServiceLifetime lifetime)
        where TImplementation : TService =>
        lifetime == ServiceLifetime.Transient &&
        typeof(IDisposable).IsAssignableFrom(typeof(TImplementation)) &&
        !typeof(IDisposable).IsAssignableFrom(typeof(TService));
}

internal interface IServicePartRegistration
{
    IEnumerable<Type> Dependencies { get; }
    
    Type ImplementationType { get; }
    
    bool IsCaching { get; }

    ServiceLifetime Lifetime { get; }
}

internal interface IServicePartRegistration<out TService> : IServicePartRegistration where TService : class
{
    TService Resolve(ServiceContext context, IReadOnlyCollection<object> dependencies);

    TService Resolve(ServiceContext context, params object[] dependencies) =>
        Resolve(context, (IReadOnlyCollection<object>)dependencies);
}

internal interface IServiceRegistration
{
    object Resolve(ServiceContext context);

    IEnumerable ResolveAll(ServiceContext context);
}

internal interface IServiceRegistration<in TService> : IServiceRegistration where TService : class
{
    void Add(IServicePartRegistration<TService> registration);
    
    void Register(TService instance);

    void Register<TImplementation>(ServiceLifetime lifetime) where TImplementation : TService;

    void Register(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime);
}

internal static class ExtensionsForIServiceRegistration
{
    private const BindingFlags Binding = BindingFlags.Static | BindingFlags.NonPublic;

    internal static void Add<TImplementation>(this IServiceRegistration registration,
        IServicePartRegistration<TImplementation> partRegistration)
        where TImplementation : class
    {
        Type registrationType = registration.GetType();
        if (!registrationType.IsGenericType) throw new InvalidOperationException();

        Type serviceType = registrationType.GetGenericArguments().Single();
        if (!serviceType.IsAssignableFrom(typeof(TImplementation))) throw new InvalidOperationException();

        typeof(ExtensionsForIServiceRegistration).GetMethod(nameof(AddTyped), Binding)!
            .MakeGenericMethod(serviceType, typeof(TImplementation))
            .Invoke(default, new object[] { registration, partRegistration });
    }

    internal static void Register<TService>(this IServiceRegistration registration, TService instance)
        where TService : class
    {
        if (registration is not IServiceRegistration<TService> typedRegistration) throw new InvalidOperationException();

        typedRegistration.Register(instance);
    }

    internal static void Register<TService>(this IServiceRegistration registration, ServiceLifetime lifetime)
        where TService : class
    {
        if (registration is not IServiceRegistration<TService> typedRegistration) throw new InvalidOperationException();

        typedRegistration.Register<TService>(lifetime);
    }

    private static void AddTyped<TService, TImplementation>(this IServiceRegistration registration,
        IServicePartRegistration<TImplementation> partRegistration)
        where TService : class
        where TImplementation : class, TService =>
        ((IServiceRegistration<TService>)registration).Add(partRegistration);
}
