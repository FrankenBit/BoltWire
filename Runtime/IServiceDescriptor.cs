namespace FrankenBit.BoltWire;

public interface IServiceDescriptor
{
    internal void Configure(IServiceRegistry registry);
    string? Key { get; }
    ServiceLifetime Lifetime { get; }
}