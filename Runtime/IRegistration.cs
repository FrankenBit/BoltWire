using System;
using System.Collections.Generic;

namespace FrankenBit.BoltWire;

internal interface IRegistration
{
    IEnumerable<Type> Dependencies { get; }
            
    ServiceLifetime Lifetime { get; }

    object GetInstance(IDictionary<Type, object> parameters);
}