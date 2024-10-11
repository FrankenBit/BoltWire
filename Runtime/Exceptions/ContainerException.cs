using System;

namespace FrankenBit.BoltWire.Exceptions;

public abstract class ContainerException : Exception
{
    protected ContainerException(Type serviceType, string message)
        : base(message) =>
        ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));

    protected ContainerException(Type serviceType, string message,
        Exception innerException)
        : base(message, innerException) =>
        ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        
    public Type ServiceType { get; }
}
