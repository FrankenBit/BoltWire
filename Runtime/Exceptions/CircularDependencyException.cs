using System;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire.Exceptions
{
    public sealed class CircularDependencyException : ServiceCompositionException
    {
        internal CircularDependencyException([NotNull] Type serviceType, [NotNull] Type dependencyType)
            : base(serviceType, dependencyType, $"Recursive composition detected for service of type {dependencyType}.")
        {
        }
    }
}
