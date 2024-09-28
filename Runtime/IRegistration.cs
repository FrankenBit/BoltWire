using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    internal interface IRegistration
    {
        [ItemNotNull]
        [NotNull]
        IEnumerable<Type> Dependencies { get; }
            
        ServiceLifetime Lifetime { get; }

        [NotNull]
        object GetInstance([NotNull] IDictionary<Type, object> parameters);
    }
}
