using System;
using System.Collections.Generic;

namespace FrankenBit.BoltWire;

public sealed class Scope : IScope
{
    private readonly CompositeDisposable _disposables = new();

    private readonly IContainer _container;

    private readonly IScope? _parent;

    private readonly Dictionary<Type, object> _scopedInstances = new();

    internal Scope(IContainer container, IScope? parent = default)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _parent = parent;
    }

    public IScope CreateScope() =>
        new Scope(_container, this);

    public void Dispose()
    {
        _disposables.Dispose();
        _scopedInstances.Clear();
    }

    public object GetService(Type serviceType)
    {
        if (TryGetExistingInstance(serviceType, out object service)) return service;

        object instance = _container.GetService(serviceType);
        if (_container.GetLifetime(serviceType) != ServiceLifetime.Scoped) return instance;
            
        _scopedInstances[serviceType] = instance;
        if (instance is IDisposable disposable) _disposables.Add(disposable);

        return instance;
    }

    public bool TryGetExistingInstance(Type serviceType, out object instance) =>
        _scopedInstances.TryGetValue(serviceType, out instance) ||
        _parent?.TryGetExistingInstance(serviceType, out instance) == true;
}