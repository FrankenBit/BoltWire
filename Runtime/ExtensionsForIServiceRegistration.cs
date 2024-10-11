using System;
using System.Linq;
using System.Reflection;

namespace FrankenBit.BoltWire;

internal static class ExtensionsForIServiceRegistration
{
    private const BindingFlags Binding = BindingFlags.Static | BindingFlags.NonPublic;

    internal static void Add<TImplementation>(this IServiceRegistration registration,
        IServicePartRegistration<TImplementation> partRegistration)
        where TImplementation : class
    {
        Type registrationType = registration.GetType();
        if (!registrationType.IsGenericType) throw new InvalidOperationException();

        Type serviceType = registrationType.GetGenericArguments().Single();
        if (!serviceType.IsAssignableFrom(typeof(TImplementation))) throw new InvalidOperationException();

        typeof(ExtensionsForIServiceRegistration).GetMethod(nameof(AddTyped), Binding)!
            .MakeGenericMethod(serviceType, typeof(TImplementation))
            .Invoke(default, new object[] { registration, partRegistration });
    }

    internal static void Register<TService>(this IServiceRegistration registration, TService instance)
        where TService : class
    {
        if (registration is not IServiceRegistration<TService> typedRegistration) throw new InvalidOperationException();

        typedRegistration.Register(instance);
    }

    internal static void Register<TService>(this IServiceRegistration registration, ServiceLifetime lifetime)
        where TService : class
    {
        if (registration is not IServiceRegistration<TService> typedRegistration) throw new InvalidOperationException();

        typedRegistration.Register<TService>(lifetime);
    }

    private static void AddTyped<TService, TImplementation>(this IServiceRegistration registration,
        IServicePartRegistration<TImplementation> partRegistration)
        where TService : class
        where TImplementation : class, TService =>
        ((IServiceRegistration<TService>)registration).Add(partRegistration);
}