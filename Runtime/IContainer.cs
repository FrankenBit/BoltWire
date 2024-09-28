using System;

namespace FrankenBit.BoltWire;

public interface IContainer : IServiceProvider
{
    ServiceLifetime GetLifetime(Type serviceType);
}