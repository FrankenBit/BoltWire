using System;
using System.Collections.Generic;
using FrankenBit.BoltWire.Exceptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FrankenBit.BoltWire
{
    internal sealed class Registry : IDisposable, IRegistry, IRegistrations
    {
        private readonly IConstructorSelector _constructorSelector;

        private readonly CompositeDisposable _disposables = new();
        
        private readonly Dictionary<Type, IRegistration> _registrations = new();

        private readonly Dictionary<Type, ICollectionRegistration> _collections = new();

        internal Registry([NotNull] IConstructorSelector constructorSelector) =>
            _constructorSelector = constructorSelector ?? throw new ArgumentNullException(nameof(constructorSelector));

        public void Dispose() =>
            _disposables.Dispose();

        public void Decorate<TService, TDecorator>() where TDecorator : TService
        {
            Debug.LogWarning("Decorate is not implemented");
        }

        public void Register<TService, TImplementation>(ServiceLifetime lifetime) where TImplementation : TService
        {
            if (lifetime == ServiceLifetime.Transient && IsHidingDisposable<TService, TImplementation>())
                throw HiddenDisposableRegistrationException.Of<TService, TImplementation>();
            
            AddRegistration<TService>(new ReflectionRegistration(
                _constructorSelector.SelectConstructor<TImplementation>(), lifetime));
        }

        public void Register<TService>(TService singleton)
        {
            IRegistration registration = new SingletonRegistration<TService>(singleton);
            _registrations[typeof(TService)] = registration;
        }

        public void RegisterFactory<TService>(Func<TService> factory, ServiceLifetime lifetime) =>
            AddRegistration<TService>(new ParameterlessFactoryRegistration<TService>(factory, lifetime));

        public void RegisterFactory<TImplementation>(Type serviceType, Func<IServiceProvider, TImplementation> factory,
            ServiceLifetime lifetime = ServiceLifetime.Transient) =>
            AddRegistration(serviceType,
                new FactoryRegistration<TImplementation>(new Resolver(this), factory, lifetime));

        public bool TryGetRegistration(Type serviceType, out IRegistration registration) =>
            TryGetDirectRegistration(serviceType, out registration) ||
            IsCollectionType(serviceType, out Type itemType) &&
            TryGetCollectionRegistration(itemType, out registration);

        internal ServiceLifetime GetLifetime(Type serviceType) =>
            _registrations.TryGetValue(serviceType, out IRegistration registration)
                ? registration.Lifetime
                : ServiceLifetime.Transient;

        [ContractAnnotation("=> true, itemType: notnull; => false, itemType: null")]
        private static bool IsCollectionType([NotNull] Type serviceType, out Type itemType)
        {
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                itemType = serviceType.GetGenericArguments()[0];
                return true;
            }

            itemType = default;
            return false;
        }

        private static bool IsHidingDisposable<TService, TImplementation>() where TImplementation : TService =>
            typeof(IDisposable).IsAssignableFrom(typeof(TImplementation)) &&
            !typeof(IDisposable).IsAssignableFrom(typeof(TService));

        private void AddRegistration<TService>([NotNull] IRegistration registration) =>
            AddRegistration(typeof(TService), registration);

        private void AddRegistration([NotNull] Type serviceType, [NotNull] IRegistration registration)
        {
            registration = WrapInSingletonIfNeeded(registration);

            if (_registrations.TryGetValue(serviceType, out IRegistration existingRegistration))
            {
                AddToCollection(serviceType, existingRegistration, registration);
            }

            _registrations[serviceType] = registration;
        }

        private void AddToCollection([NotNull] Type serviceType, [NotNull] IRegistration existingRegistration,
            [NotNull] IRegistration newRegistration)
        {
            if (!_collections.TryGetValue(serviceType, out ICollectionRegistration collection))
            {
                collection = new CollectionRegistration(serviceType, existingRegistration);
                _collections[serviceType] = collection;
            }

            collection.Add(newRegistration);
        }

        private SingletonCacheRegistration CreateCache(IRegistration registration)
        {
            var cache = new SingletonCacheRegistration(registration);
            _disposables.Add(cache);
            return cache;
        }

        private bool TryGetCollectionRegistration(Type serviceType, out IRegistration registration)
        {
            registration = default;
            if (!_collections.TryGetValue(serviceType, out ICollectionRegistration collectionRegistration))
                return false;

            registration = collectionRegistration;
            return true;
        }

        private bool TryGetDirectRegistration(Type serviceType, out IRegistration registration) =>
            _registrations.TryGetValue(serviceType, out registration);

        [NotNull]
        private IRegistration WrapInSingletonIfNeeded([NotNull] IRegistration registration) =>
            registration.Lifetime == ServiceLifetime.Singleton ? CreateCache(registration) : registration;
    }

    internal class FactoryRegistration<TImplementation> : IRegistration
    {
        [NotNull]
        private readonly Func<IServiceProvider, TImplementation> _factory;

        [NotNull]
        private readonly IServiceProvider _resolver;

        public FactoryRegistration([NotNull] IServiceProvider resolver,
            [NotNull] Func<IServiceProvider, TImplementation> factory, ServiceLifetime lifetime)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Lifetime = lifetime;
        }

        public IEnumerable<Type> Dependencies =>
            Array.Empty<Type>();
        
        public ServiceLifetime Lifetime { get; }

        public object GetInstance(IDictionary<Type, object> parameters) =>
            _factory.Invoke(_resolver);
    }
}
