using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FrankenBit.BoltWire;

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