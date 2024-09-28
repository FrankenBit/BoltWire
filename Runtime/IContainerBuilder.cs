using System;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    public interface IContainerBuilder
    {
        internal void AddRegistrationSetup([NotNull] Action<IRegistry> registrationSetup);
    }
}
