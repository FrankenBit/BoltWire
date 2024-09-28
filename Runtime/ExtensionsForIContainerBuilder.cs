using System;
using System.Linq;
using FrankenBit.BoltWire.Exceptions;
using UnityEngine;

namespace FrankenBit.BoltWire
{
    public static class ExtensionsForIContainerBuilder
    {
        public static IContainerBuilder<TService> AsImplementedInterfaces<TService>(
            this IContainerBuilder<TService> builder) where TService : class
        {
            foreach (Type serviceInterface in typeof(TService).GetInterfaces())
            {
                builder.AddRegistrationSetup(registry =>
                    registry.RegisterFactory(serviceInterface, container => container.Resolve<TService>()));
            }

            return builder;
        }

        public static TBuilder Register<TBuilder>(this TBuilder builder, DependencySetup<TBuilder> dependencySetup)
            where TBuilder : IContainerBuilder =>
            dependencySetup.Invoke(builder);

        public static IContainerBuilder<TService>
            Register<TService>(this IContainerBuilder builder, ServiceLifetime lifetime) =>
            builder.Register<TService, TService>(lifetime);

        public static IContainerBuilder<TService> Register<TService, TImplementation>(this IContainerBuilder builder,
            ServiceLifetime lifetime) where TImplementation : TService
        {
            builder.AddRegistrationSetup(registry => registry.Register<TService, TImplementation>(lifetime));
            return new ContainerBuilder<TService>(builder);
        }

        public static IContainerBuilder<TService> RegisterComponentInHierarchy<TService>(
            this IContainerBuilder builder)
        {
            builder.AddRegistrationSetup(registry =>
                registry.Register(
                    UnityEngine.Object
                        .FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
                        .OfType<TService>().SingleOrDefault() ??
                    throw ComponentNotFoundInHierarchyException.Create<TService>()));
            return new ContainerBuilder<TService>(builder);
        }

        public static IContainerBuilder<TService> RegisterComponentInHierarchy<TService, TImplementation>(
            this IContainerBuilder builder) where TImplementation : TService
        {
            builder.AddRegistrationSetup(registry => registry.Register<TService>(UnityEngine.Object
                .FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
                .OfType<TImplementation>().Single()));
            return new ContainerBuilder<TService>(builder);
        }

        public static IContainerBuilder<TService> RegisterScoped<TService>(this IContainerBuilder builder) =>
            Register<TService>(builder, ServiceLifetime.Scoped);

        public static IContainerBuilder<TService> RegisterScoped<TService, TImplementation>(
            this IContainerBuilder builder)
            where TImplementation : TService =>
            Register<TService, TImplementation>(builder, ServiceLifetime.Scoped);

        public static IContainerBuilder<TService> WithDependencies<TService>(
            this IContainerBuilder<TService> builder, DependencySetup<IContainerBuilder> dependencySetup) =>
            throw new NotImplementedException();
    }
}