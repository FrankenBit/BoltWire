using System;
using System.Collections.Generic;
using System.Linq;

namespace FrankenBit.BoltWire;

/// <summary>
///     Represents a collection of disposable objects.
/// </summary>
public sealed class CompositeDisposable : IDisposable
{
    private readonly List<IDisposable> _items;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CompositeDisposable"/> class.
    /// </summary>
    public CompositeDisposable() =>
        _items = new List<IDisposable>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="CompositeDisposable"/> class.
    /// </summary>
    /// <param name="items">
    ///     The items to add to the collection.
    /// </param>
    public CompositeDisposable(IEnumerable<IDisposable> items) =>
        _items = items.ToList();

    /// <inheritdoc />
    public void Dispose() =>
        _items.ForEach(Dispose);

    /// <summary>
    ///     Adds an item to the collection.
    /// </summary>
    /// <param name="item">
    ///     The item to add.
    /// </param>
    public void Add(IDisposable item) =>
        _items.Add(item);

    private static void Dispose(IDisposable item) =>
        item.Dispose();
}