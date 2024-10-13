using NUnit.Framework;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class TestServiceDoesNotImplementAnyInterfacesException
{
    [Test]
    public void Ctor_WithParams_ProvidesExpectedMessage() =>
        Assert.That(ServiceDoesNotImplementAnyInterfacesException.For<TestService>().Message,
        Is.EqualTo("TestService does not implement any interfaces."));
}