using System.Diagnostics.CodeAnalysis;

namespace FrankenBit.BoltWire;

public interface IPendingService<in TService> : IServiceCollection where TService : class
{
    public IPendingService<TService> DecorateWith<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicConstructors)] TDecorator>()
        where TDecorator : TService
    {
        this.Register<TService, TDecorator>(Lifetime, Key);
        return this;
    }

    string? Key { get; }

    ServiceLifetime Lifetime { get; }
}
