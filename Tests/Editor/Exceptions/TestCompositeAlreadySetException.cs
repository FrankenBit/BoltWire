using FrankenBit.BoltWire.Stubs;
using NUnit.Framework;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class TestCompositeAlreadySetException
{
    [Test]
    public void Ctor_WithParams_ProvidesExpectedMessage() =>
        Assert.That(CompositeAlreadySetException.For<ITestService>().Message,
        Is.EqualTo("Composite for ITestService is already set."));
}