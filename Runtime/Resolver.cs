using System;
using System.Collections.Generic;
using System.Linq;
using FrankenBit.BoltWire.Exceptions;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    internal sealed class Resolver : IServiceProvider
    {
        private readonly Stack<Type> _pendingTypes = new();

        private readonly Dictionary<Type, object> _resolvedInstances = new();

        private readonly HashSet<Type> _resolvingTypes = new();

        private readonly IRegistrations _registrations;

        internal Resolver([NotNull] IRegistrations registrations) =>
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

        [NotNull]
        private object GetInstance([NotNull] Type serviceType) =>
            _resolvedInstances[serviceType];

        [NotNull]
        private IDictionary<Type, object> GetInstances([ItemNotNull] [NotNull] IEnumerable<Type> dependencies) =>
            dependencies.ToDictionary(dependency => dependency, GetInstance);

        private bool GotMissingDependency([NotNull] Type serviceType,
            [ItemNotNull] [NotNull] IEnumerable<Type> dependencies) =>
            TryGetMissingDependency(dependencies, out Type missing) && AddMissingDependency(serviceType, missing);

        private bool HasInstance([NotNull] Type serviceType) =>
            _resolvedInstances.ContainsKey(serviceType);

        private bool IsAlreadyResolving([NotNull] Type serviceType) =>
            _resolvingTypes.Contains(serviceType);

        private void ResolveTree()
        {
            while (_pendingTypes.Count > 0 && ResolveBranchIfNeeded(_pendingTypes.Pop()))
            {
            }
        }

        private bool ResolveBranchIfNeeded([NotNull] Type serviceType) =>
            HasInstance(serviceType) ||
            TryGetRegistration(serviceType, out IRegistration registration) &&
            ResolveBranch(serviceType, registration);

        private bool ResolveBranch([NotNull] Type serviceType, [NotNull] IRegistration registration)
        {
            _resolvingTypes.Add(serviceType);
            return ResolveBranch(serviceType, registration, registration.Dependencies.ToList());
        }

        private bool ResolveBranch([NotNull] Type serviceType, [NotNull] IRegistration registration,
            [ItemNotNull] [NotNull] IReadOnlyCollection<Type> dependencies) =>
            GotMissingDependency(serviceType, dependencies) ||
            ResolveInstance(serviceType, registration, dependencies);

        private bool ResolveInstance([NotNull] Type serviceType, [NotNull] IRegistration registration,
            [ItemNotNull] [NotNull] IEnumerable<Type> dependencies)
        {
            SetInstance(serviceType, registration.GetInstance(GetInstances(dependencies)));
            _resolvingTypes.Remove(serviceType);
            return true;
        }

        private void SetInstance([NotNull] Type serviceType, [NotNull] object serviceInstance) =>
            _resolvedInstances[serviceType] = serviceInstance;

        private bool ThrowIfDependency([NotNull] Type serviceType) =>
            _pendingTypes.Any()
                ? throw new UnresolvedDependencyException(_pendingTypes.Peek(), serviceType)
                : false;

        [ContractAnnotation("=> true, missing: notnull; => false, missing: null")]
        private bool TryGetMissingDependency([ItemNotNull] [NotNull] IEnumerable<Type> dependencies,
            out Type missing)
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

        [ContractAnnotation("=> true, registration: notnull; => false, registration: null")]
        private bool TryGetRegistration([NotNull] Type serviceType, out IRegistration registration) =>
            _registrations.TryGetRegistration(serviceType, out registration) ||
            ThrowIfDependency(serviceType);
    }
}