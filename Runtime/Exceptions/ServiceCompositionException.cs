using System;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire.Exceptions
{
    public abstract class ServiceCompositionException : ServiceResolutionException
    {
        protected ServiceCompositionException([NotNull] Type serviceType, [NotNull] Type dependencyType,
            [NotNull] string message)
            : base(serviceType, message) =>
            DependencyType = dependencyType ?? throw new ArgumentNullException(nameof(dependencyType));

        protected ServiceCompositionException([NotNull] Type serviceType, [NotNull] Type dependencyType,
            [NotNull] string message, [NotNull] Exception innerException)
            : base(serviceType, message, innerException) =>
            DependencyType = dependencyType;

        [NotNull]
        public Type DependencyType { get; }
    }
}
