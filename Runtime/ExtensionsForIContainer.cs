namespace FrankenBit.BoltWire;

public static class ExtensionsForIContainer
{
    public static IScope CreateScope(this IContainer container) =>
        new Scope(container);
}