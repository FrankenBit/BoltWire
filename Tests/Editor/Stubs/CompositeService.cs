using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FrankenBit.BoltWire.Stubs;

internal sealed class CompositeService : IOtherTestService, IEnumerable<ITestService>
{
    private readonly List<ITestService> _items;

    internal CompositeService(IEnumerable<ITestService> items) =>
        _items = items.ToList();

    public IEnumerator<ITestService> GetEnumerator() =>
        _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
