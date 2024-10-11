using System;
using System.Reflection;

namespace FrankenBit.BoltWire;

public interface IConstructorSelector
{
    ConstructorInfo SelectConstructor(Type implementationType);
}
