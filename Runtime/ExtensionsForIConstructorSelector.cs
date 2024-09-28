using System.Reflection;

namespace FrankenBit.BoltWire;

internal static class ExtensionsForIConstructorSelector
{
    internal static ConstructorInfo
        SelectConstructor<TImplementation>(this IConstructorSelector selector) =>
        selector.SelectConstructor(typeof(TImplementation));
}