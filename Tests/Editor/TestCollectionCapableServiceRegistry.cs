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
    private readonly StubRegistry _stubRegistry = new();

    [Test]
    public void GetRegistration_WithParams_ForwardsToBaseRegistry()
    {
        _ = new CollectionCapableServiceRegistry(_stubRegistry).GetRegistration<object>(null);

        Assert.That(_stubRegistry.GetRegistrationCallCount, Is.EqualTo(1));
    }

    [Test]
    public void TryGetRegistration_Collection_RegistryHasPrecedence()
    {
        _ = new CollectionCapableServiceRegistry(_stubRegistry).TryGetRegistration(typeof(IEnumerable<TestService>),
                 default, out IServiceRegistration? registration);

        Assert.That(registration, Is.SameAs(_stubRegistry.CollectionRegistration));
    }

    private sealed class StubRegistry : IServiceRegistry
    {
        internal IServiceRegistration CollectionRegistration { get; } =
            new StubRegistration<IEnumerable<TestService>>();

        internal int GetRegistrationCallCount { get; private set; }

        public IServiceRegistration<TService> GetRegistration<TService>(string? key) where TService : class
        {
            GetRegistrationCallCount++;
            return new StubRegistration<TService>();
        }

        public ConstructorInfo SelectConstructor(Type implementationType) =>
            throw new NotImplementedException();

        public bool TryGetRegistration(Type serviceType, string? key,
            [NotNullWhen(true)] out IServiceRegistration? registration)
        {
            registration = serviceType == typeof(IEnumerable<TestService>)
                ? CollectionRegistration
                : null;
            return registration is not null;
        }
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
