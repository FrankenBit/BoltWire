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