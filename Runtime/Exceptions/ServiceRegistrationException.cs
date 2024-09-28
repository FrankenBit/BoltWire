using System;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire.Exceptions
{
    public abstract class ServiceRegistrationException : ContainerException
    {
        protected ServiceRegistrationException([NotNull] Type serviceType, [NotNull] string message)
            : base(serviceType, message)
        {
        }

        protected ServiceRegistrationException([NotNull] Type serviceType, [NotNull] string message,
            [NotNull] Exception innerException)
            : base(serviceType, message, innerException)
        {
        }
    }
}
