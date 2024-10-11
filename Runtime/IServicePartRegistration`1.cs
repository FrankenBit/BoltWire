using System.Collections.Generic;

namespace FrankenBit.BoltWire;

internal interface IServicePartRegistration<out TService> : IServicePartRegistration where TService : class
{
    TService Resolve(ServiceContext context, IReadOnlyCollection<object> dependencies);

    TService Resolve(ServiceContext context, params object[] dependencies) =>
        Resolve(context, (IReadOnlyCollection<object>)dependencies);
}
