using System;

namespace FrankenBit.BoltWire;

public interface IContainerBuilder
{
    internal void AddRegistrationSetup(Action<IRegistry> registrationSetup);
}