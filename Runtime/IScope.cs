using System;
using JetBrains.Annotations;

namespace FrankenBit.BoltWire
{
    public interface IScope : IDisposable, IServiceProvider
    {
        [NotNull]
        IScope CreateScope();

        [ContractAnnotation("=> true, instance: notnull; => false, instance: null")]
        bool TryGetExistingInstance([NotNull] Type serviceType, out object instance); 
    }
}
