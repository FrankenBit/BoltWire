using System;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire;

public interface IScope : IDisposable, IServiceProvider
{
    IScope CreateScope();

    [ContractAnnotation("=> true, instance: notnull; => false, instance: null")]
    bool TryGetExistingInstance(Type serviceType, out object instance); 
}