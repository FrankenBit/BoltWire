using System;
using System.Collections.Generic;

namespace FrankenBit.BoltWire;

internal sealed class SingletonCacheRegistration<TService> : CacheRegistrationBase<TService> where TService : class
{
    private TService? _instance;

    internal SingletonCacheRegistration(IServicePartRegistration<TService> registration)
        : base(registration)
    {
    }

    public override IEnumerable<Type> Dependencies =>
        _instance is null ? base.Dependencies : Array.Empty<Type>();

    protected override TService? GetCachedInstance(IServiceContext context) =>
        _instance ??= base.GetCachedInstance(context);
}
