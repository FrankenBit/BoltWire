using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    public sealed class ContainerBuilder : IContainerBuilder
    {
        private readonly List<Action<IRegistry>> _registrationSetups = new();

        [CanBeNull]
        private RegistrationBuilder _pendingBuilder;

        public Container Build()
        {
            CommitPendingBuilder(default);

            var registry = new Registry(new GreedyConstructorSelector());
            var container = new Container(registry);
            foreach (Action<IRegistry> registrationSetup in _registrationSetups)
            {
                registrationSetup.Invoke(registry);
            }

            return container;
        }

        void IContainerBuilder.AddRegistrationSetup(Action<IRegistry> registrationSetup) =>
            CommitPendingBuilder(new RegistrationBuilder(registrationSetup));

        private void CommitPendingBuilder([CanBeNull] RegistrationBuilder nextBuilder)
        {
            if (_pendingBuilder != null) _registrationSetups.Add(_pendingBuilder.RegistrationSetup);

            _pendingBuilder = nextBuilder;
        }

        private sealed class RegistrationBuilder
        {
            internal RegistrationBuilder([NotNull] Action<IRegistry> registrationSetup) =>
                RegistrationSetup = registrationSetup ?? throw new ArgumentNullException(nameof(registrationSetup));

            internal Action<IRegistry> RegistrationSetup { get; }
        }
    }
}
