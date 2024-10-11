using System.Collections.Generic;
using System.Linq;

namespace FrankenBit.BoltWire;

/// <summary>
///     Represents a collection of <see cref="IStartable" /> items that can be started.
/// </summary>
public sealed class CompositeStartable : IStartable
{
    private readonly List<IStartable> _items;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CompositeStartable" /> class.
    /// </summary>
    public CompositeStartable() =>
        _items = new List<IStartable>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="CompositeStartable" /> class.
    /// </summary>
    /// <param name="items">
    ///     The items to start.
    /// </param>
    public CompositeStartable(IEnumerable<IStartable> items) =>
        _items = items.ToList();

    /// <summary>
    ///     Adds an item to the collection.
    /// </summary>
    /// <param name="item">
    ///     The item to add.
    /// </param>
    public void Add(IStartable item) =>
        _items.Add(item);

    /// <summary>
    ///     Adds a range of items to the collection.
    /// </summary>
    /// <param name="items">
    ///     The items to add.
    /// </param>
    public void AddRange(IEnumerable<IStartable> items) =>
        _items.AddRange(items);

    /// <inheritdoc />
    public void Start() =>
        _items.ForEach(Start);

    private static void Start(IStartable item) =>
        item.Start();
}