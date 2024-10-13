using System;
using System.Collections.Generic;

namespace FrankenBit.BoltWire;

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

    public TService Resolve(IServiceContext context, IReadOnlyCollection<object> dependencies) =>
        _instance;
}