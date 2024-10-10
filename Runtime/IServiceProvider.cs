using System;

namespace FrankenBit.BoltWire;

public interface IServiceProvider : System.IServiceProvider
{
    object? System.IServiceProvider.GetService(Type serviceType) =>
        GetService(serviceType, default);
    
    object? GetService(Type serviceType, string? key);
}