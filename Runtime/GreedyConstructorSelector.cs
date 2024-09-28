using System;
using System.Linq;
using System.Reflection;

namespace FrankenBit.BoltWire
{
    public sealed class GreedyConstructorSelector : IConstructorSelector
    {
        private const BindingFlags DefaultBindingFlags =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

        private readonly BindingFlags _bindingFlags;

        public GreedyConstructorSelector(BindingFlags bindingFlags = DefaultBindingFlags) =>
            _bindingFlags = bindingFlags;

        public ConstructorInfo SelectConstructor(Type implementationType) =>
            implementationType.GetConstructors(_bindingFlags)
                .OrderByDescending(c => c.GetParameters().Length)
                .First();
    }
}