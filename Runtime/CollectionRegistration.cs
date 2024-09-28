using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FrankenBit.BoltWire;

internal sealed class CollectionRegistration : ICollectionRegistration
{
    private readonly List<IRegistration> _registrations = new();

    private readonly IList _list;

    internal CollectionRegistration(Type serviceType, IRegistration existingRegistration)
    {
        if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
        if (existingRegistration == null) throw new ArgumentNullException(nameof(existingRegistration));

        _registrations.Add(existingRegistration);
            
        Type listType = typeof(List<>).MakeGenericType(serviceType);
        _list = (IList)Activator.CreateInstance(listType);
    }

    public IEnumerable<Type> Dependencies =>
        _registrations.SelectMany(GetDependencies).Distinct();

    public ServiceLifetime Lifetime =>
        ServiceLifetime.Transient;

    public void Add(IRegistration registration) =>
        _registrations.Add(registration);

    public object GetInstance(IDictionary<Type, object> parameters)
    {
        _list.Clear();
        foreach (object instance in GetInstances(_registrations, parameters))
            _list.Add(instance);
        return _list;
    }

    private static IEnumerable<Type> GetDependencies(IRegistration registration) =>
        registration.Dependencies;

    private static IEnumerable<object> GetInstances(List<IRegistration> registrations,
        IDictionary<Type, object> parameters) =>
        registrations.Select(registration => registration.GetInstance(parameters));
}