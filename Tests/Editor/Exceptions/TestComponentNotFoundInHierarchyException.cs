using FrankenBit.BoltWire.Stubs;
using NUnit.Framework;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class TestComponentNotFoundInHierarchyException
{
    [Test]
    public void Ctor_WithParams_ProvidesExpectedMessage() =>
        Assert.That(ComponentNotFoundInHierarchyException.For<TestService>().Message,
        Is.EqualTo("TestService component not found in hierarchy."));
}