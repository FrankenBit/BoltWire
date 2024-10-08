﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FrankenBit.BoltWire;

internal sealed class ReflectionRegistration : IRegistration
{
    private readonly ConstructorInfo _constructor;

    internal ReflectionRegistration(ConstructorInfo constructor, ServiceLifetime lifetime)
    {
        _constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
        Lifetime = lifetime;
                
        Dependencies = _constructor.GetParameters().Select(p => p.ParameterType).ToList();
    }

    public IEnumerable<Type> Dependencies { get; }

    public ServiceLifetime Lifetime { get; }

    public object GetInstance(IDictionary<Type, object> parameters) =>
        _constructor.Invoke(Dependencies.Select(dependency => parameters[dependency]).ToArray());
}