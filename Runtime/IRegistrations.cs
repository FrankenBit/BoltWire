using System;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    internal interface IRegistrations
    {
        [ContractAnnotation("=> true, registration: notnull; => false, registration: null")]
        bool TryGetRegistration([NotNull] Type serviceType, out IRegistration registration);
    }
}
