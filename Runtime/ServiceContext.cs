using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FrankenBit.BoltWire.Exceptions;

namespace FrankenBit.BoltWire;

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