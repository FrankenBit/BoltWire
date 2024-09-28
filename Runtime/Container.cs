using System;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    public sealed class Container : IContainer, IDisposable
    {
        private readonly Registry _registry;

        internal Container([NotNull] Registry registry) =>
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));

        public void Dispose() =>
            _registry.Dispose();

        public ServiceLifetime GetLifetime(Type serviceType) =>
            _registry.GetLifetime(serviceType);

        public object GetService(Type serviceType)
        {
            var resolver = new Resolver(_registry);
            return resolver.GetService(serviceType);
        }
    }
}
