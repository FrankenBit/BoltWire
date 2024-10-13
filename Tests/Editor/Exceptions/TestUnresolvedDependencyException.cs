using NUnit.Framework;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class TestUnresolvedDependencyException
{
    [Test]
    public void Ctor_WithParams_ProvidesExpectedMessage() =>
        Assert.That(UnresolvedDependencyException.For<ITestService, TestService>().Message,
        Is.EqualTo($"ITestService dependency TestService could not be resolved."));
}