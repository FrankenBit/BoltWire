using System;
using System.Collections.Generic;

namespace FrankenBit.BoltWire;

internal sealed class SingletonRegistration<TInstance> : IRegistration
{
    private readonly TInstance _instance;

    internal SingletonRegistration(TInstance instance)
    {
        _instance = instance ?? throw new ArgumentNullException(nameof(instance));
    }

    public IEnumerable<Type> Dependencies =>
        Array.Empty<Type>();

    public ServiceLifetime Lifetime =>
        ServiceLifetime.Singleton;

    public object GetInstance(IDictionary<Type, object> parameters) =>
        _instance!;
}
