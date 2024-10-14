using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FrankenBit.BoltWire.Stubs;
using NUnit.Framework;

namespace FrankenBit.BoltWire;

public sealed class TestCollectionCapableServiceRegistry
{
    [Test]
    public void GetRegistration_WithParams_ForwardsToBaseRegistry()
    {
        var registry = new StubRegistry();

        _ = new CollectionCapableServiceRegistry(registry).GetRegistration<object>(null);

        Assert.That(registry.GetRegistrationCallCount, Is.EqualTo(1));
    }

    [Test]
    public void SelectConstructor_WithParams_ForwardsToBaseRegistry()
    {
        var registry = new StubRegistry();

        _ = new CollectionCapableServiceRegistry(registry).SelectConstructor(typeof(TestService));

        Assert.That(registry.SelectConstructorCallCount, Is.EqualTo(1));
    }

    [Test]
    public void TryGetRegistration_Collection_RegistryHasPrecedence()
    {
        var registry = new StubRegistry();

        _ = new CollectionCapableServiceRegistry(registry).TryGetRegistration(typeof(IEnumerable<TestService>),
                 default, out IServiceRegistration? registration);

        Assert.That(registration, Is.SameAs(registry.CollectionRegistration));
    }

    [Test]
    public void TryGetRegistration_NonCollection_ForwardsOnlyOnce()
    {
        var registry = new StubRegistry();

        _ = new CollectionCapableServiceRegistry(registry).TryGetRegistration(typeof(TestService),
                 default, out IServiceRegistration? _);

        Assert.That(registry.TryGetRegistrationCallCount, Is.EqualTo(1));
    }

    [Test]
    public void TryGetRegistration_NonCollection_ReturnsFalse()
    {
        var registry = new StubRegistry();

        bool result = new CollectionCapableServiceRegistry(registry).TryGetRegistration(typeof(TestService),
                 default, out IServiceRegistration? _);

        Assert.That(result, Is.False);
    }

    [Test]
    public void TryGetRegistration_RegisteredType_ReturnsTrue()
    {
        var registry = new StubRegistry();

        bool result = new CollectionCapableServiceRegistry(registry)
            .TryGetRegistration(typeof(IEnumerable<ITestService>), default, out IServiceRegistration? _);

        Assert.That(result, Is.True);
    }

    [Test]
    public void TryGetRegistration_UnregisteredType_ReturnsFalse()
    {
        var registry = new StubRegistry();

        bool result = new CollectionCapableServiceRegistry(registry)
            .TryGetRegistration(typeof(IEnumerable<OtherTestService>), default, out IServiceRegistration? _);

        Assert.That(result, Is.False);
    }

    private sealed class StubRegistry : IServiceRegistry
    {
        internal IServiceRegistration CollectionRegistration { get; } =
            new StubRegistration<IEnumerable<TestService>>();

        internal int GetRegistrationCallCount { get; private set; }

        internal int SelectConstructorCallCount { get; private set; }

        internal int TryGetRegistrationCallCount { get; private set; }

        public IServiceRegistration<TService> GetRegistration<TService>(string? key) where TService : class
        {
            GetRegistrationCallCount++;
            return new StubRegistration<TService>();
        }

        public ConstructorInfo SelectConstructor(Type implementationType)
        {
            SelectConstructorCallCount++;
            return implementationType.GetConstructors()[0];
        }

        public bool TryGetRegistration(Type serviceType, string? key,
            [NotNullWhen(true)] out IServiceRegistration? registration)
        {
            TryGetRegistrationCallCount++;
            registration = GetRegistration(serviceType);
            return registration is not null;
        }

        private IServiceRegistration? GetRegistration(Type serviceType) =>
            serviceType switch
            {
                _ when serviceType == typeof(ITestService) => new StubRegistration<ITestService>(),
                _ when serviceType == typeof(IEnumerable<TestService>) => CollectionRegistration,
                _ => null
            };
    }

    private sealed class StubRegistration<TService> : IServiceRegistration<TService> where TService : class
    {
        public void Add(IServicePartRegistration<TService> registration)
        {
            throw new NotImplementedException();
        }

        public void Register<TImplementation>(ServiceLifetime lifetime) where TImplementation : TService
        {
            throw new NotImplementedException();
        }

        public void Register(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime)
        {
            throw new NotImplementedException();
        }

        public object Resolve(ServiceContext context) =>
            throw new NotImplementedException();

        public void Register(TService instance)
        {
            throw new NotImplementedException();
        }

        IEnumerable IServiceRegistration.ResolveAll(ServiceContext context) =>
            throw new NotImplementedException();
    }
}
