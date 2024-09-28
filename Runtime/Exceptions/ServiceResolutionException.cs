using System;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire.Exceptions
{
    public abstract class ServiceResolutionException : ContainerException
    {
        protected ServiceResolutionException([NotNull] Type serviceType, string message)
            : base(serviceType, message)
        {
        }

        protected ServiceResolutionException([NotNull] Type serviceType, [NotNull] string message,
            [NotNull] Exception innerException)
            : base(serviceType, message, innerException)
        {
        }
    }
}
