using System;
using System.Collections.Generic;

namespace FrankenBit.BoltWire;

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

    public TService Resolve(IServiceContext context, IReadOnlyCollection<object> dependencies) =>
        GetCachedInstance(context) ?? _registration.Resolve(context, dependencies);
    
    protected virtual TService? GetCachedInstance(IServiceContext context) =>
        context.TryGetInstance(ImplementationType, out object? instance)
            ? (TService)instance
            : default; 
}
