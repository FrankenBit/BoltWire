using System.Collections;

namespace FrankenBit.BoltWire;

internal interface IServiceRegistration
{
    object Resolve(ServiceContext context);

    IEnumerable ResolveAll(ServiceContext context);
}