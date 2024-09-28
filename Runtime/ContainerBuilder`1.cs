using System;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    internal sealed class ContainerBuilder<TService> : IContainerBuilder<TService>
    {
        private readonly IContainerBuilder _builder;

        internal ContainerBuilder([NotNull] IContainerBuilder builder) =>
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));

        public IContainerBuilder<TService> DecorateWith<TDecorator>() where TDecorator : TService
        {
            _builder.AddRegistrationSetup(registry => registry.Decorate<TService, TDecorator>());
            return this;
        }

        void IContainerBuilder.AddRegistrationSetup(Action<IRegistry> registrationSetup) =>
            _builder.AddRegistrationSetup(registrationSetup);
    }
}
