using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FrankenBit.BoltWire;

internal static class CompositeType
{
    private const string GenericCollectionsNamespace =
        $"{nameof(System)}.{nameof(System.Collections)}.{nameof(System.Collections.Generic)}";

    private const string GenericEnumerationName = $"{nameof(IEnumerable)}`1";

    private static readonly Type CollectionBaseType = typeof(IEnumerable);

    internal static bool IsComposite<TService>(IEnumerable<Type> dependencies) where TService : class =>
        dependencies.Where(CollectionBaseType.IsAssignableFrom)
            .Select(FlattenToGenericIEnumerable)
            .Where(NotNull)
            .Select(GetItemType!)
            .Any(IsCollectionOf<TService>);

    internal static bool TryGetItemType(Type serviceType, [NotNullWhen(true)] out Type? itemType)
    {
        itemType = default;
        if (!CollectionBaseType.IsAssignableFrom(serviceType)) return false;

        Type? enumerationType = FlattenToGenericIEnumerable(serviceType);
        if (enumerationType is null) return false;

        itemType = GetItemType(serviceType);
        return true;
    }

    private static Type? FlattenToGenericIEnumerable(Type type) =>
        IsGenericIEnumerable(type) ? type : type.GetInterfaces().SingleOrDefault(IsGenericIEnumerable);

    private static Type GetItemType(Type collectionType) =>
        collectionType.GenericTypeArguments[0];

    private static bool IsCollectionOf<TService>(Type? itemType) =>
        typeof(TService).IsAssignableFrom(itemType) ||
        typeof(TService).GetInterfaces().Any(i => i.IsAssignableFrom(itemType));

    private static bool IsGenericIEnumerable(Type type) =>
        GenericEnumerationName.Equals(type.Name) && GenericCollectionsNamespace.Equals(type.Namespace);

    private static bool NotNull(Type? type) =>
        type is not null;
}
