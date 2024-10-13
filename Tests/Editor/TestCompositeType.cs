using System;
using System.Collections;
using System.Collections.Generic;
using FrankenBit.BoltWire.Stubs;
using NUnit.Framework;

namespace FrankenBit.BoltWire;

public sealed class TestCompositeType
{
    private static readonly Type[] ConcreteCompositeEnumeration = { typeof(IEnumerable<TestService>) };

    private static readonly Type[] InterfaceCompositeEnumeration = { typeof(IEnumerable<ITestService>) };

    private static readonly Type[] CompositeService = { typeof(CompositeService) };

    private static readonly Type[] ConcreteCompositeList = { typeof(List<TestService>) };

    private static readonly Type[] InterfaceCompositeList = { typeof(List<ITestService>) };

    private static readonly Type[] NonCompositeDependencies = { typeof(TestService) };

    private static readonly Type[] NonGenericDependency = { typeof(IEnumerable) };

    private static readonly Type[] NonEnumerationDependency = { typeof(Comparer<>) };

    private static readonly Type[] OtherConcreteCompositeList = { typeof(List<OtherTestService>) };

    private static readonly Type[] StrangeThings = { typeof(StrangeThing) };

    private static readonly Type[] TwoCollections = { typeof(List<OtherTestService>), typeof(List<TestService>) };

    [Test]
    public void IsComposite_AsInterfaceWithConcreteCompositeEnumeration_ReturnsTrue() =>
        Assert.That(CompositeType.IsComposite<ITestService>(ConcreteCompositeEnumeration), Is.True);

    [Test]
    public void IsComposite_AsInterfaceWithInterfaceCompositeEnumeration_ReturnsTrue() =>
        Assert.That(CompositeType.IsComposite<ITestService>(InterfaceCompositeEnumeration), Is.True);

    [Test]
    public void IsComposite_AsInterfaceWithConcreteCompositeList_ReturnsTrue() =>
        Assert.That(CompositeType.IsComposite<ITestService>(ConcreteCompositeList), Is.True);

    [Test]
    public void IsComposite_AsInterfaceWithInterfaceCompositeList_ReturnsTrue() =>
        Assert.That(CompositeType.IsComposite<ITestService>(InterfaceCompositeList), Is.True);

    [Test]
    public void IsComposite_AsInterfaceWithNonCompositeDependencies_ReturnsFalse() =>
        Assert.That(CompositeType.IsComposite<ITestService>(NonCompositeDependencies), Is.False);

    [Test]
    public void IsComposite_OtherTestServiceWithCompositeService_ReturnsFalse() =>
        Assert.That(CompositeType.IsComposite<OtherTestService>(CompositeService), Is.False);

    [Test]
    public void IsComposite_TestServiceWithCompositeService_ReturnsTrue() =>
        Assert.That(CompositeType.IsComposite<TestService>(CompositeService), Is.True);

    [Test]
    public void IsComposite_WithConcreteCompositeEnumeration_ReturnsTrue() =>
        Assert.That(CompositeType.IsComposite<TestService>(ConcreteCompositeEnumeration), Is.True);

    [Test]
    public void IsComposite_WithInterfaceCompositeEnumeration_ReturnsTrue() =>
        Assert.That(CompositeType.IsComposite<TestService>(InterfaceCompositeEnumeration), Is.True);

    [Test]
    public void IsComposite_WithNonCompositeDependencies_ReturnsFalse() =>
        Assert.That(CompositeType.IsComposite<TestService>(NonCompositeDependencies), Is.False);

    [Test]
    public void IsComposite_WithNonEnumerationDependency_ReturnsFalse() =>
        Assert.That(CompositeType.IsComposite<TestService>(NonEnumerationDependency), Is.False);

    [Test]
    public void IsComposite_WithNonGenericDependency_ReturnsFalse() =>
        Assert.That(CompositeType.IsComposite<TestService>(NonGenericDependency), Is.False);

    [Test]
    public void IsComposite_WithOtherConcreteCompositeList_ReturnsFalse() =>
        Assert.That(CompositeType.IsComposite<TestService>(OtherConcreteCompositeList), Is.False);

    [Test]
    public void IsComposite_WithStrangeThings_ReturnsFalse() =>
        Assert.That(CompositeType.IsComposite<TestService>(StrangeThings), Is.False);

    [Test]
    public void IsComposite_WithTwoCollections_ReturnsTrue() =>
        Assert.That(CompositeType.IsComposite<TestService>(TwoCollections), Is.True);

    [Test]
    public void TryGetItemType_AsCollection_ReturnsTrue() =>
        Assert.That(CompositeType.TryGetItemType(typeof(IEnumerable<TestService>), out Type? _), Is.True);

    [Test]
    public void TryGetItemType_AsCollection_ReturnsItemType()
    {
        bool _ = CompositeType.TryGetItemType(typeof(IEnumerable<TestService>), out Type? itemType);
        Assert.That(itemType, Is.SameAs(typeof(TestService)));
    }

    [Test]
    public void TryGetItemType_AsInterfaceCollection_ReturnsItemType()
    {
        bool _ = CompositeType.TryGetItemType(typeof(IEnumerable<ITestService>), out Type? itemType);
        Assert.That(itemType, Is.SameAs(typeof(ITestService)));
    }

    [Test]
    public void TryGetItemType_AsNonCollection_ReturnsFalse() =>
        Assert.That(CompositeType.TryGetItemType(typeof(TestService), out Type? _), Is.False);

    [Test]
    public void TryGetItemType_StrangeThing_ReturnsFalse() =>
        Assert.That(CompositeType.TryGetItemType(typeof(StrangeThing), out Type? _), Is.False);

    private sealed class StrangeThing : IEnumerable, IComparer<ITestService>
    {
        public IEnumerator GetEnumerator() =>
            throw new InvalidOperationException();

        public int Compare(ITestService? x, ITestService? y) =>
            throw new InvalidOperationException();
    }
}
