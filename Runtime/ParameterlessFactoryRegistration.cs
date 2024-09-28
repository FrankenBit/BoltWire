using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    internal sealed class ParameterlessFactoryRegistration<TInstance> : IRegistration
    {
        [NotNull]
        private readonly Func<TInstance> _factory;

        internal ParameterlessFactoryRegistration([NotNull] Func<TInstance> factory, ServiceLifetime lifetime)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Lifetime = lifetime;
        }

        public IEnumerable<Type> Dependencies =>
            Array.Empty<Type>();

        public ServiceLifetime Lifetime { get; }

        public object GetInstance(IDictionary<Type, object> parameters) =>
            _factory();
    }
}
