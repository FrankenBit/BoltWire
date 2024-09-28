using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire;

internal sealed class SingletonCacheRegistration : IDisposable, IRegistration
{
    private readonly IRegistration _registration;

    private IDisposable? _disposable;
        
    private object? _instance;

    internal SingletonCacheRegistration(IRegistration registration) =>
        _registration = registration ?? throw new ArgumentNullException(nameof(registration));

    public IEnumerable<Type> Dependencies =>
        _instance is null ? _registration.Dependencies : Array.Empty<Type>();

    public ServiceLifetime Lifetime =>
        ServiceLifetime.Singleton;

    public void Dispose()
    {
        _disposable?.Dispose();
        _instance = _disposable = default;
    }

    public object GetInstance(IDictionary<Type, object> parameters) =>
        _instance ??= CreateInstance(parameters);

    private object CreateInstance(IDictionary<Type, object> parameters) =>
        StoreIfDisposable(_registration.GetInstance(parameters));

    private object StoreIfDisposable(object instance)
    {
        _disposable = instance as IDisposable;
        return instance;
    }
}