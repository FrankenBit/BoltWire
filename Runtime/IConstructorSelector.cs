using System;
using System.Reflection;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    public interface IConstructorSelector
    {
        [NotNull]
        ConstructorInfo SelectConstructor([NotNull] Type implementationType);
    }
}
