using System;
using System.Diagnostics.CodeAnalysis;

namespace FrankenBit.BoltWire;

/// <summary>
///     Represents a service provider that can resolve services by type and key.
/// </summary>
/// <remarks>
///     Extends <see cref="System.IServiceProvider"/> by adding a key parameter
///     to the <see cref="GetService"/> method.
/// </remarks>
[SuppressMessage("NDepend", "ND2012:AvoidHavingDifferentTypesWithSameName",
Justification = "Interface is a superset of System.IServiceProvider")]
public interface IServiceProvider : System.IServiceProvider
{
    /// <inheritdoc cref="System.IServiceProvider.GetService" />
    object? System.IServiceProvider.GetService(Type serviceType) =>
        GetService(serviceType, default);

    /// <summary>
    ///     Gets the service instance of the specified type and key.
    /// </summary>
    /// <param name="serviceType">
    ///     The type of service instance to get.
    /// </param>
    /// <param name="key">
    ///     An optional key of the service instance to get.
    /// </param>
    /// <returns>
    ///     The service instance of the specified type and key, or <see langword="null" />
    ///     if the service is not found.
    /// </returns>
    object? GetService(Type serviceType, string? key);
}
