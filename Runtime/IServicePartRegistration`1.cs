using System.Collections.Generic;

namespace FrankenBit.BoltWire;

internal interface IServicePartRegistration<out TService> : IServicePartRegistration where TService : class
{
    TService Resolve(IServiceContext context, IReadOnlyCollection<object> dependencies);

    TService Resolve(IServiceContext context, params object[] dependencies) =>
        Resolve(context, (IReadOnlyCollection<object>)dependencies);
}
