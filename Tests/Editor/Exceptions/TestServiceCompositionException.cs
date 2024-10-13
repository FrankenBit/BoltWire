using System;
using NUnit.Framework;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class TestServiceCompositionException
{
    [Test]
    public void Ctor_WithParams_ProvidesExpectedServiceType() =>
        Assert.That(new StubException(typeof(ITestService), typeof(TestService)).ServiceType,
        Is.SameAs(typeof(ITestService)));

    [Test]
    public void Ctor_WithParams_ProvidesExpectedDependencyType() =>
        Assert.That(new StubException(typeof(ITestService), typeof(TestService)).DependencyType,
        Is.SameAs(typeof(TestService)));

    private sealed class StubException : ServiceCompositionException
    {
        internal StubException(Type serviceType, Type dependencyType)
            : base(serviceType, dependencyType, "Stub")
        {
        }
    }
}