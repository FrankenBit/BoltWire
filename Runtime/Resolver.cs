using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FrankenBit.BoltWire.Exceptions;

namespace FrankenBit.BoltWire;

internal sealed class Resolver : IServiceProvider
{
    private readonly Stack<Type> _pendingTypes = new();

    private readonly Dictionary<Type, object> _resolvedInstances = new();

    private readonly HashSet<Type> _resolvingTypes = new();

    private readonly IRegistrations _registrations;

    internal Resolver(IRegistrations registrations) =>
        _registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));

    public object GetService(Type serviceType)
    {
        _pendingTypes.Push(serviceType);
        ResolveTree();

        object result = _resolvedInstances.GetValueOrDefault(serviceType);

        Clear();
        return result;
    }

    private bool AddMissingDependency(Type serviceType, Type missingDependency)
    {
        if (IsAlreadyResolving(missingDependency))
            throw new CircularDependencyException(serviceType, missingDependency);

        _pendingTypes.Push(serviceType);
        _pendingTypes.Push(missingDependency);
        return true;
    }

    private void Clear()
    {
        _pendingTypes.Clear();
        _resolvedInstances.Clear();
        _resolvingTypes.Clear();
    }

    private object GetInstance(Type serviceType) =>
        _resolvedInstances[serviceType];

    private IDictionary<Type, object> GetInstances(IEnumerable<Type> dependencies) =>
        dependencies.ToDictionary(dependency => dependency, GetInstance);

    private bool GotMissingDependency(Type serviceType,
        IEnumerable<Type> dependencies) =>
        TryGetMissingDependency(dependencies, out Type? missing) && AddMissingDependency(serviceType, missing);

    private bool HasInstance(Type serviceType) =>
        _resolvedInstances.ContainsKey(serviceType);

    private bool IsAlreadyResolving(Type serviceType) =>
        _resolvingTypes.Contains(serviceType);

    private void ResolveTree()
    {
        while (_pendingTypes.Count > 0 && ResolveBranchIfNeeded(_pendingTypes.Pop()))
        {
        }
    }

    private bool ResolveBranchIfNeeded(Type serviceType) =>
        HasInstance(serviceType) ||
        TryGetRegistration(serviceType, out IRegistration? registration) &&
        ResolveBranch(serviceType, registration);

    private bool ResolveBranch(Type serviceType, IRegistration registration)
    {
        _resolvingTypes.Add(serviceType);
        return ResolveBranch(serviceType, registration, registration.Dependencies.ToList());
    }

    private bool ResolveBranch(Type serviceType, IRegistration registration,
        IReadOnlyCollection<Type> dependencies) =>
        GotMissingDependency(serviceType, dependencies) ||
        ResolveInstance(serviceType, registration, dependencies);

    private bool ResolveInstance(Type serviceType, IRegistration registration,
        IEnumerable<Type> dependencies)
    {
        SetInstance(serviceType, registration.GetInstance(GetInstances(dependencies)));
        _resolvingTypes.Remove(serviceType);
        return true;
    }

    private void SetInstance(Type serviceType, object serviceInstance) =>
        _resolvedInstances[serviceType] = serviceInstance;

    private bool ThrowIfDependency(Type serviceType) =>
        _pendingTypes.Any()
            ? throw new UnresolvedDependencyException(_pendingTypes.Peek(), serviceType)
            : false;

    private bool TryGetMissingDependency(IEnumerable<Type> dependencies, [NotNullWhen(true)] out Type? missing)
    {
        foreach (Type dependency in dependencies)
        {
            if (HasInstance(dependency)) continue;

            missing = dependency;
            return true;
        }

        missing = default;
        return false;
    }

    private bool TryGetRegistration(Type serviceType, [NotNullWhen(true)] out IRegistration? registration) =>
        _registrations.TryGetRegistration(serviceType, out registration) ||
        ThrowIfDependency(serviceType);
}