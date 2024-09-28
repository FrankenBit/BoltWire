using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    internal interface ICollectionRegistration : IRegistration
    {
        void Add([NotNull] IRegistration registration);
    }
}
