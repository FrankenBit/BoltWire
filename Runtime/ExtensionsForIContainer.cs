using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    public static class ExtensionsForIContainer
    {
        [NotNull]
        public static IScope CreateScope([NotNull] this IContainer container) =>
            new Scope(container);
    }
}
