using System;
using System.Diagnostics.CodeAnalysis;

namespace FrankenBit.BoltWire;

public interface IScope : IDisposable, IServiceProvider
{
    IScope CreateScope();

    bool TryGetService(Type serviceType, string? key, [NotNullWhen(true)] out object? instance);
}