namespace FrankenBit.BoltWire;

internal sealed class ScopedCacheRegistration<TService> : CacheRegistrationBase<TService> where TService : class
{
    internal ScopedCacheRegistration(IServicePartRegistration<TService> registration)
        : base(registration)
    {
    }
}