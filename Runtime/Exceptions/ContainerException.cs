using System;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire.Exceptions
{
    public abstract class ContainerException : Exception
    {
        protected ContainerException([NotNull] Type serviceType, [NotNull] string message)
            : base(message) =>
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));

        protected ContainerException([NotNull] Type serviceType, [NotNull] string message,
            [NotNull] Exception innerException)
            : base(message, innerException) =>
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        
        [NotNull]
        public Type ServiceType { get; }
    }
}
