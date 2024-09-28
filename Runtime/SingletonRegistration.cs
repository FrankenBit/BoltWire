using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    internal sealed class SingletonRegistration<TInstance> : IRegistration
    {
        [NotNull]
        private readonly TInstance _instance;

        internal SingletonRegistration([NotNull] TInstance instance)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public IEnumerable<Type> Dependencies =>
            Array.Empty<Type>();

        public ServiceLifetime Lifetime =>
            ServiceLifetime.Singleton;
        public object GetInstance(IDictionary<Type, object> parameters) =>
            _instance;
    }
}
