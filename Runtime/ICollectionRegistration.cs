namespace FrankenBit.BoltWire;

internal interface ICollectionRegistration : IRegistration
{
    void Add(IRegistration registration);
}