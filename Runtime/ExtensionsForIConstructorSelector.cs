using System.Reflection;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    internal static class ExtensionsForIConstructorSelector
    {
        [NotNull]
        internal static ConstructorInfo
            SelectConstructor<TImplementation>([NotNull] this IConstructorSelector selector) =>
            selector.SelectConstructor(typeof(TImplementation));
    }
}