using System;
using System.Diagnostics.CodeAnalysis;

namespace FrankenBit.BoltWire;

internal interface IServiceRegistry : IConstructorSelector
{
    IServiceRegistration<TService> GetRegistration<TService>(string? key) where TService : class;
    
    bool TryGetRegistration(Type serviceType, string? key, [NotNullWhen(true)] out IServiceRegistration? registration);
}
