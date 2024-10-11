using System;
using System.Collections.Generic;

namespace FrankenBit.BoltWire;

internal interface IServicePartRegistration
{
    IEnumerable<Type> Dependencies { get; }
    
    Type ImplementationType { get; }
    
    bool IsCaching { get; }

    ServiceLifetime Lifetime { get; }
}
