using System;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire.Exceptions
{
    public sealed class HiddenDisposableRegistrationException : ServiceRegistrationException
    {
        private HiddenDisposableRegistrationException([NotNull] Type serviceType, [NotNull] Type implementationType)
            : base(serviceType, $"Transient registration of {implementationType} as {serviceType} hides IDisposable.") =>
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));

        [NotNull]
        public Type ImplementationType { get; }

        internal static HiddenDisposableRegistrationException Of<TService, TImplementation>() =>
            new(typeof(TService), typeof(TImplementation));
    }
}
