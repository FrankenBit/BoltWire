using System;
using NUnit.Framework;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class TestContainerException
{
    [Test]
    public void Ctor_WithParams_ProvidesExpectedType() =>
        Assert.That(new StubException(typeof(TestService)).ServiceType,
        Is.SameAs(typeof(TestService)));

    private sealed class StubException : ContainerException
    {
        internal StubException(Type serviceType)
            : base(serviceType, "Stub")
        {
        }
    }
}