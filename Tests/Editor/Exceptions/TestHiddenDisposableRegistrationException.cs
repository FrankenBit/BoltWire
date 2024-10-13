using NUnit.Framework;

namespace FrankenBit.BoltWire.Exceptions;

public sealed class TestHiddenDisposableRegistrationException
{
    [Test]
    public void Ctor_WithParams_ProvidesExpectedMessage() =>
        Assert.That(HiddenDisposableRegistrationException.For<ITestService, TestService>().Message,
        Is.EqualTo("Transient registration of TestService as ITestService would hide IDisposable."));

    [Test]
    public void ImplementationType_Property_ProvidesExpectedType() =>
        Assert.That(HiddenDisposableRegistrationException.For<ITestService, TestService>().ImplementationType,
        Is.EqualTo(typeof(TestService)));
}