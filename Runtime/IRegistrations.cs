using System;
using System.Diagnostics.CodeAnalysis;

namespace FrankenBit.BoltWire;

internal interface IRegistrations
{
    bool TryGetRegistration(Type serviceType, [NotNullWhen(true)] out IRegistration? registration);
}