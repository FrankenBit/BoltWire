namespace FrankenBit.BoltWire;

internal static class ExtensionsForIServicePartRegistration
{
    internal static IServicePartRegistration<TService> CacheIfNeeded<TService>(
        this IServicePartRegistration<TService> registration) where TService : class =>
        registration.IsCaching ? registration : Cache(registration);

    private static IServicePartRegistration<TService> Cache<TService>(IServicePartRegistration<TService> registration)
        where TService : class =>
        registration.Lifetime switch
        {
            ServiceLifetime.Singleton => new SingletonCacheRegistration<TService>(registration),
            ServiceLifetime.Scoped => new ScopedCacheRegistration<TService>(registration),
            _ => registration
        };
}