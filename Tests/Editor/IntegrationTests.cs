using System;
using System.Collections.Generic;
using System.Linq;
using FrankenBit.BoltWire.Exceptions;
using NUnit.Framework;

namespace FrankenBit.BoltWire;

public sealed class IntegrationTests
{
    [Test]
    public void Dispose_Scope_DoesDisposeScopedInstance()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder
            .Register<Bar>(ServiceLifetime.Scoped)
            .Register<Foo>(ServiceLifetime.Scoped);
        using Container container = containerBuilder.Build();
        Foo? foo;
        using (IScope scope = container.CreateScope())
        {
            foo = scope.Resolve<Foo>();
        }

        Assert.That(foo?.Disposed, Is.True);
    }

    [Test]
    public void Dispose_Singleton_DoesDisposeSingletonInstance()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder
            .Register<Bar>(ServiceLifetime.Singleton)
            .Register<Foo>(ServiceLifetime.Singleton);
        Foo? foo;
        using (Container container = containerBuilder.Build())
        {
            foo = container.Resolve<Foo>();
        }

        Assert.That(foo?.Disposed, Is.True);
    }
        
    [Test]
    public void Resolve_Composite_DoesResolveComposite()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder
            .Register<ITestInterface, Foo>(ServiceLifetime.Scoped)
            .Register<ITestInterface, Bar>(ServiceLifetime.Scoped)
            .Register<ITestInterface, TestComposite>(ServiceLifetime.Scoped);
        using Container container = containerBuilder.Build();

        var actual = container.Resolve<ITestInterface>();

        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public void Register_TransientHidingDisposable_DoesThrow()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register<ITestInterface, Foo>(ServiceLifetime.Transient);

        Assert.That(() => containerBuilder.Build(), Throws.InstanceOf<HiddenDisposableRegistrationException>());
    }

    [Test]
    public void Resolve_AsImplementedInterfaces_DoesResolve()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder
            .RegisterScoped<Bar>()
            .Register<Foo>(ServiceLifetime.Singleton).AsImplementedInterfaces();
        using Container container = containerBuilder.Build();

        var actual1 = container.Resolve<ITestInterface>();
        var actual2 = container.Resolve<IDisposable>();

        Assert.That(actual1, Is.Not.Null.And.SameAs(actual2));
    }

    [Test]
    public void Resolve_CompositeDependency_DoesResolveAll()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder
            .Register<ITestInterface, Foo>(ServiceLifetime.Scoped)
            .Register<ITestInterface, Bar>(ServiceLifetime.Scoped)
            .Register<Bar>(ServiceLifetime.Scoped)
            .Register<Baz>(ServiceLifetime.Scoped);
        using Container container = containerBuilder.Build();

        var actual = container.Resolve<Baz>();

        Assert.That(actual?.PartCount, Is.EqualTo(2));
    }

    [Test]
    public void Resolve_DependentService_DoesResolveServices()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder
            .Register<Foo>(ServiceLifetime.Scoped)
            .Register<Bar>(ServiceLifetime.Scoped);
        using Container container = containerBuilder.Build();

        var actual = container.Resolve<Bar>();

        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public void Resolve_DependentServiceAsInterface_DoesResolveInterface()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder
            .Register<ITestInterface, Foo>(ServiceLifetime.Scoped)
            .Register<Bar>(ServiceLifetime.Scoped);
        using Container container = containerBuilder.Build();

        var actual = container.Require<ITestInterface>();

        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public void Resolve_DirectDependencyCycle_DoesThrow()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register<Ying>(ServiceLifetime.Scoped);
        containerBuilder.Register<Yang>(ServiceLifetime.Scoped);
        using Container container = containerBuilder.Build();

        Assert.That(() => container.Resolve<Ying>(), Throws.InstanceOf<CircularDependencyException>());
    }

    [Test]
    public void Resolve_ManuallyBuiltContainerWithDependencyChain_DoesResolveServices()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder
            .Register<Foo>(ServiceLifetime.Scoped)
            .Register<Bar>(ServiceLifetime.Scoped);
        using Container container = containerBuilder.Build();

        var actual = container.Resolve<Foo>();

        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public void Resolve_ManuallyBuiltContainerWithSingleService_DoesResolveServices()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register<Bar>(ServiceLifetime.Scoped);
        using Container container = containerBuilder.Build();

        var actual = container.Resolve<Bar>();

        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public void Resolve_MissingDependency_DoesThrow()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register<Foo>(ServiceLifetime.Scoped);
        using Container container = containerBuilder.Build();

        Assert.That(() => container.Resolve<Foo>(), Throws.InstanceOf<UnresolvedDependencyException>());
    }

    [Test]
    public void Resolve_NoScope_DoesResolveDifferentInstances()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register<Bar>(ServiceLifetime.Scoped);
        using Container container = containerBuilder.Build();

        var actual1 = container.Resolve<Bar>();
        var actual2 = container.Resolve<Bar>();

        Assert.That(actual1, Is.Not.SameAs(actual2));
    }

    [Test]
    public void Resolve_ParentScope_DoesResolveDifferentInstances()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register<Bar>(ServiceLifetime.Scoped);
        using Container root = containerBuilder.Build();
        using IScope scope = root.CreateScope();

        var actual1 = root.Resolve<Bar>();
        var actual2 = scope.Resolve<Bar>();

        Assert.That(actual1, Is.Not.SameAs(actual2));
    }

    [Test]
    public void Resolve_SameScope_DoesResolveSameInstance()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register<Bar>(ServiceLifetime.Scoped);
        using Container container = containerBuilder.Build();
        using IScope scope = container.CreateScope();

        var actual1 = scope.Resolve<Bar>();
        var actual2 = scope.Resolve<Bar>();

        Assert.That(actual1, Is.SameAs(actual2));
    }

    [Test]
    public void Resolve_ServiceImplementation_DoesNotResolveWrongService()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register<Bar>(ServiceLifetime.Scoped);
        using Container container = containerBuilder.Build();
            
        var actual = container.Resolve<Foo>();

        Assert.That(actual, Is.Null);
    }

    [Test]
    public void Resolve_ServiceImplementation_DoesResolveCorrectService()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register<Bar>(ServiceLifetime.Scoped);
        using Container container = containerBuilder.Build();
            
        var actual = container.Resolve<Bar>();

        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public void Resolve_Singleton_SameInstance()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register<Bar>(ServiceLifetime.Singleton);
        using Container container = containerBuilder.Build();

        var actual1 = container.Resolve<Bar>();
        var actual2 = container.Resolve<Bar>();

        Assert.That(actual1, Is.SameAs(actual2));
    }

    [Test]
    public void Resolve_Transient_DifferentInstances()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register<Bar>(ServiceLifetime.Transient);
        using Container container = containerBuilder.Build();

        var actual1 = container.Resolve<Bar>();
        var actual2 = container.Resolve<Bar>();

        Assert.That(actual1, Is.Not.SameAs(actual2));
    }

    [Test]
    public void ResolveAll_MultipleRegistrations_ReturnsAllInstances()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register<Bar>(ServiceLifetime.Scoped);
        containerBuilder.Register<ITestInterface, Bar>(ServiceLifetime.Scoped);
        containerBuilder.Register<ITestInterface, Foo>(ServiceLifetime.Scoped);
        using Container container = containerBuilder.Build();

        IEnumerable<ITestInterface> actual = container.ResolveAll<ITestInterface>();

        Assert.That(actual.Count(), Is.EqualTo(2));
    }

    private interface ITestInterface
    {
        bool Disposed { get; }
            
        string GetText();
    }
        
    private sealed class Bar : ITestInterface
    {
        public bool Disposed =>
            false;

        public string GetText() =>
            "Bar";
    }

    private sealed class Baz
    {
        private readonly ITestInterface[] _parts;

        internal Baz(IEnumerable<ITestInterface> parts) =>
            _parts = parts.ToArray();

        internal int PartCount =>
            _parts.Length;
    }

    private sealed class Foo : ITestInterface, IDisposable
    {
        private readonly Bar _bar;

        internal Foo(Bar bar) =>
            _bar = bar;

        public bool Disposed { get; private set; }

        public void Dispose() =>
            Disposed = true;

        public string GetText() =>
            $"Foo{_bar.GetText()}";
    }

    private sealed class TestComposite : ITestInterface
    {
        private readonly IReadOnlyCollection<ITestInterface> _items;

        public TestComposite(IEnumerable<ITestInterface> items) =>
            _items = items.ToArray();

        public bool Disposed =>
            _items.Any(item => item.Disposed);
            
        public string GetText() =>
            string.Join(", ", _items.Select(item => item.GetText()));
    }
        
    private sealed class Ying : ITestInterface
    {
        private readonly Yang _dependency;

        internal Ying(Yang dependency) =>
            _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));

        public bool Disposed =>
            false;

        public string GetText() =>
            $"Ying"+_dependency.GetText();
    }
        
    private sealed class Yang : ITestInterface
    {
        private readonly Ying _dependency;

        internal Yang(Ying dependency) =>
            _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));

        public bool Disposed =>
            false;

        public string GetText() =>
            $"{_dependency.GetText()}Yang";
    }
}