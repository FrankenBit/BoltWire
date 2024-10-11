using System;
using System.Diagnostics.CodeAnalysis;

namespace FrankenBit.BoltWire;

internal interface IInstanceTracker
{
    void Track(object service, ServiceLifetime lifetime, string? key = default)
    {
        if (service is IDisposable disposable) TrackDisposable(disposable, lifetime);
        TrackInstance(service, lifetime, key);
    }

    void TrackDisposable(IDisposable service, ServiceLifetime lifetime);

    void TrackInstance(object service, ServiceLifetime lifetime, string? key);
    
    bool TryGetService(Type serviceType, string? key, [NotNullWhen(true)] out object? instance);
}