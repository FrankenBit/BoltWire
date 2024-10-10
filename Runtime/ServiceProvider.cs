using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FrankenBit.BoltWire.Tools;

namespace FrankenBit.BoltWire;

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