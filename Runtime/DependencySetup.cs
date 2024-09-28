namespace FrankenBit.BoltWire;

public delegate TBuilder DependencySetup<TBuilder>(TBuilder builder)
    where TBuilder : IContainerBuilder;