using NUnit.Framework;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class TestComponentNotFoundInHierarchyException
{
    [Test]
    public void Ctor_WithParams_ProvidesExpectedMessage()
    {
        Assert.That(ComponentNotFoundInHierarchyException.Create<TestService>().Message,
        Is.EqualTo("Component of type TestService not found in hierarchy."));
    }
}
