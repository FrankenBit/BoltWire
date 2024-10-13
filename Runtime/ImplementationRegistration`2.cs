using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FrankenBit.BoltWire.Exceptions;

namespace FrankenBit.BoltWire;

internal sealed class ImplementationRegistration<TService, TImplementation> : IServicePartRegistration<TService>
    where TService : class where TImplementation : TService
{
    private readonly ConstructorInfo _constructor;

    private readonly Type[] _dependencies;

    internal ImplementationRegistration(ConstructorInfo constructor, IEnumerable<Type> dependencies,
        ServiceLifetime lifetime)
    {
        _constructor = constructor;
        _dependencies = dependencies.ToArray();
        Lifetime = lifetime;
    }

    public IEnumerable<Type> Dependencies =>
        _dependencies;

    public Type ImplementationType =>
        typeof(TImplementation);

    public bool IsCaching =>
        false;

    public ServiceLifetime Lifetime { get; }

    public TService Resolve(IServiceContext context, IReadOnlyCollection<object> dependencies) =>
        context.Track(Create(context, dependencies), Lifetime);

    private TService Create(IServiceContext context, IReadOnlyCollection<object> dependencies)
    {
        Dictionary<Type, object> suppliedDependencies = dependencies.ToDictionary(instance => instance.GetType());

        return (TService)_constructor.Invoke(_dependencies.Select(ResolveDependency).ToArray());

        Type? FindBestSuitableType(Type type) =>
            suppliedDependencies.ContainsKey(type)
                ? type
                : suppliedDependencies.Keys.FirstOrDefault(type.IsAssignableFrom);

        object ResolveDependency1(Type dependencyType, Type? bestSuppliedType) =>
            bestSuppliedType is not null
                ? suppliedDependencies[bestSuppliedType]
                : context.GetDependency(typeof(TImplementation), dependencyType, context.Key) ??
                  throw new UnresolvedDependencyException(typeof(TImplementation), dependencyType);
        
        object ResolveDependency(Type dependencyType) =>
            ResolveDependency1(dependencyType, FindBestSuitableType(dependencyType));
    }
}