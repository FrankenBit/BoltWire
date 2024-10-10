using System;
using System.Linq;
using System.Reflection;
using FrankenBit.BoltWire.Exceptions;

namespace FrankenBit.BoltWire;

internal static class ImplementationRegistration
{
    internal static IServicePartRegistration<TService> Create<TService, TImplementation>(
        IConstructorSelector constructorSelector, ServiceLifetime lifetime)
        where TService : class
        where TImplementation : TService
    {
        if (IsTransientHidingDisposable<TService, TImplementation>(lifetime))
            throw HiddenDisposableRegistrationException.Of<TService, TImplementation>();

        ConstructorInfo constructor = constructorSelector.SelectConstructor<TImplementation>();
        Type[] dependencies = constructor.GetParameters()
            .Select(parameter => parameter.ParameterType)
            .ToArray();
        return new ImplementationRegistration<TService, TImplementation>(constructor, dependencies, lifetime);
    }

    private static bool IsTransientHidingDisposable<TService, TImplementation>(ServiceLifetime lifetime)
        where TImplementation : TService =>
        lifetime == ServiceLifetime.Transient &&
        typeof(IDisposable).IsAssignableFrom(typeof(TImplementation)) &&
        !typeof(IDisposable).IsAssignableFrom(typeof(TService));
}
