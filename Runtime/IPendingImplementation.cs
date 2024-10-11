namespace FrankenBit.BoltWire;

public interface IPendingImplementation<in TService> :
    IPendingService<TService> where TService : class
{
    public IPendingService<TService> AsImplementedInterfaces();
}