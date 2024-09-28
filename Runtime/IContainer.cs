using System;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    public interface IContainer : IServiceProvider
    {
        ServiceLifetime GetLifetime([NotNull] Type serviceType);
    }
}
