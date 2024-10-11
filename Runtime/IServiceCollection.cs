using System.Collections.Generic;

namespace FrankenBit.BoltWire;

public interface IServiceCollection : IReadOnlyCollection<IServiceDescriptor>
{
    void Add(IServiceDescriptor descriptor);

    void Remove(IServiceDescriptor descriptor);
}