using System;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire.Exceptions
{
    public sealed class ComponentNotFoundInHierarchyException : ServiceRegistrationException
    {
        private ComponentNotFoundInHierarchyException([NotNull] Type serviceType)
            : base(serviceType, $"Component of type {serviceType} not found in hierarchy")
        {
        }

        internal static Exception Create<TService>() =>
            new ComponentNotFoundInHierarchyException(typeof(TService));
    }
}
