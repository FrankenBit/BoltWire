namespace FrankenBit.BoltWire
{
    public interface IContainerBuilder<in TService> : IContainerBuilder
    {
        IContainerBuilder<TService> DecorateWith<TDecorator>() where TDecorator : TService;
    }
}
