using FrankenBit.BoltWire.Stubs;
using NUnit.Framework;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class TestServiceNotRegisteredInHierarchyException
{
    [Test]
    public void Ctor_WithParams_ProvidesExpectedMessage() =>
        Assert.That(ServiceNotRegisteredException.For<TestService>().Message,
        Is.EqualTo("TestService is not registered."));
}