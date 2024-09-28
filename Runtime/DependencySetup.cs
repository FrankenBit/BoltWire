using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    [NotNull]
    public delegate TBuilder DependencySetup<TBuilder>([NotNull] TBuilder builder)
        where TBuilder : IContainerBuilder;
}
