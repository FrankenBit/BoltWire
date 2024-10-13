using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace FrankenBit.BoltWire;

public sealed class TestCacheRegistrationBase
{
    private const ServiceLifetime TestLifetime = ServiceLifetime.Scoped;

    private static readonly Type[] TestDependencies = { typeof(StubService) };

    private readonly StubServicePartRegistration _registration = new();

    [Test]
    public void Dependencies_Property_ForwardsToBaseRegistration() =>
        Assert.That(new TestCacheRegistration(_registration).Dependencies, Is.SameAs(TestDependencies));

    [Test]
    public void ImplementationType_Property_ForwardsToBaseRegistration() =>
        Assert.That(new TestCacheRegistration(_registration).ImplementationType, Is.EqualTo(typeof(StubService)));

    [Test]
    public void IsCaching_Property_ReturnsTrue() =>
        Assert.That(new TestCacheRegistration(_registration).IsCaching, Is.True);

    [Test]
    public void Lifetime_Property_ForwardsToBaseRegistration() =>
        Assert.That(new TestCacheRegistration(_registration).Lifetime, Is.EqualTo(TestLifetime));

    [Test]
    public void Resolve_WithContextCachedInstance_ChecksForContextCacheBeforeInstantiating()
    {
        var cached = new StubService();
        var registration = new TestCacheRegistration(_registration);
        StubService instance = registration.Resolve(new StubContext(cached), Array.Empty<object>());
        Assert.That(instance, Is.SameAs(cached));
    }

    [Test]
    public void Resolve_WithLocalCachedInstance_ChecksForLocalCacheFirst()
    {
        var cached = new StubService();
        var registration = new TestCacheRegistration(_registration, cached);
        StubService instance = registration.Resolve(new StubContext(default), Array.Empty<object>());
        Assert.That(instance, Is.SameAs(cached));
    }

    [Test]
    public void Resolve_WithoutCachedInstance_ReturnsNotNull()
    {
        var registration = new TestCacheRegistration(_registration);
        var context = new StubContext(default);
        StubService instance = registration.Resolve(context, Array.Empty<object>());
        Assert.That(instance, Is.Not.Null);
    }

    private sealed class StubContext : IServiceContext
    {
        private readonly StubService? _cached;

        internal StubContext(StubService? cached) =>
            _cached = cached;

        [ExcludeFromCodeCoverage]
        public string? Key =>
            default;

        [ExcludeFromCodeCoverage]
        public object GetDependency(Type serviceType, Type dependencyType, string? key) =>
            throw new InvalidOperationException();

        [ExcludeFromCodeCoverage]
        public TService Track<TService>(TService service, ServiceLifetime lifetime) where TService : class =>
            throw new InvalidOperationException();

        public bool TryGetInstance(Type implementationType, [NotNullWhen(true)] out object? instance) =>
            (instance = _cached) is not null;
    }

    private sealed class StubService
    {
    }

    private sealed class StubServicePartRegistration : IServicePartRegistration<StubService>
    {
        public IEnumerable<Type> Dependencies =>
            TestDependencies;

        public Type ImplementationType =>
            typeof(StubService);

        [ExcludeFromCodeCoverage]
        public bool IsCaching =>
            false;

        public ServiceLifetime Lifetime =>
            TestLifetime;

        public StubService Resolve(IServiceContext context, IReadOnlyCollection<object> dependencies) =>
            new();
    }

    private sealed class TestCacheRegistration : CacheRegistrationBase<StubService>
    {
        private readonly StubService? _cached;

        internal TestCacheRegistration(IServicePartRegistration<StubService> registration, StubService? cached = default)
            : base(registration)
        {
            _cached = cached;
        }

        protected override StubService? GetCachedInstance(IServiceContext context) =>
            _cached ?? base.GetCachedInstance(context);
    }
}
