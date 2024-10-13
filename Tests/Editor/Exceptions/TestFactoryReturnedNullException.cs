using FrankenBit.BoltWire.Stubs;
using NUnit.Framework;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class TestFactoryReturnedNullException
{
    [Test]
    public void Ctor_WithParams_ProvidesExpectedMessage() =>
        Assert.That(FactoryReturnedNullException.For<ITestService>().Message,
        Is.EqualTo("Factory for ITestService returned null."));
}