using NUnit.Framework;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class TestCircularDependencyException
{
    [Test]
    public void Ctor_WithParams_ProvidesExpectedMessage()
    {
        Assert.That(new CircularDependencyException(typeof(ITestService), typeof(TestService)).Message,
        Is.EqualTo("Recursive composition detected for ITestService service dependency TestService."));
    }
}
