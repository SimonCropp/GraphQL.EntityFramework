enum MemberKind
{
    /// <summary>
    /// A comparable value. <see cref="EntityMember.Type"/> is the underlying type, nullable removed.
    /// </summary>
    Scalar,

    /// <summary>
    /// A reference navigation or complex property. <see cref="EntityMember.Type"/> is the target type.
    /// </summary>
    Reference,

    /// <summary>
    /// A collection navigation. <see cref="EntityMember.Type"/> is the item type.
    /// </summary>
    Collection
}

record EntityMember(string Name, Type Type, MemberKind Kind);

/// <summary>
/// The members of a type that the generated where and orderBy input types expose. Read from the
/// EF model when a registered service's model knows the type, so only mapped properties and
/// navigations appear, and from the public scalar properties otherwise, for projection and dto types.
/// </summary>
static class EntityShape
{
    static FrozenSet<Type> scalarTypes = FrozenSet.ToFrozenSet(
    [
        typeof(string),
        typeof(bool),
        typeof(byte),
        typeof(sbyte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(Guid),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(Date),
        typeof(Time),
    ]);

    public static IReadOnlyList<EntityMember> Members(Type type, IEnumerable<IEfGraphQLService> services)
    {
        foreach (var service in services)
        {
            var typeBase = FindTypeBase(service.Model, type);
            if (typeBase is not null)
            {
                return FromModel(typeBase);
            }
        }

        return FromClr(type);
    }

    static ITypeBase? FindTypeBase(IModel model, Type type)
    {
        // FindEntityType returns null for a shared type, such as an owned type used by two owners
        var entityType = model.FindEntityTypes(type).FirstOrDefault();
        if (entityType is not null)
        {
            return entityType;
        }

        foreach (var entity in model.GetEntityTypes())
        {
            var complex = FindComplex(entity, type);
            if (complex is not null)
            {
                return complex;
            }
        }

        return null;
    }

    static IComplexType? FindComplex(ITypeBase typeBase, Type type)
    {
        foreach (var property in typeBase.GetComplexProperties())
        {
            if (property.ComplexType.ClrType == type)
            {
                return property.ComplexType;
            }

            var nested = FindComplex(property.ComplexType, type);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }

    static IReadOnlyList<EntityMember> FromModel(ITypeBase typeBase)
    {
        var members = new List<EntityMember>();
        foreach (var property in typeBase.GetProperties())
        {
            if (property.IsShadowProperty() ||
                !IsPublic(property.PropertyInfo))
            {
                continue;
            }

            if (TryScalar(property.ClrType, out var scalar))
            {
                members.Add(new(property.Name, scalar, MemberKind.Scalar));
            }
        }

        foreach (var property in typeBase.GetComplexProperties())
        {
            if (property.IsCollection ||
                !IsPublic(property.PropertyInfo))
            {
                continue;
            }

            members.Add(new(property.Name, property.ComplexType.ClrType, MemberKind.Reference));
        }

        if (typeBase is IEntityType entityType)
        {
            var navigations = entityType.GetNavigations()
                .Cast<INavigationBase>()
                .Concat(entityType.GetSkipNavigations());
            foreach (var navigation in navigations)
            {
                if (!IsPublic(navigation.PropertyInfo))
                {
                    continue;
                }

                var target = navigation.TargetEntityType.ClrType;
                members.Add(navigation.IsCollection
                    ? new(navigation.Name, target, MemberKind.Collection)
                    : new(navigation.Name, target, MemberKind.Reference));
            }
        }

        return members
            .OrderBy(_ => _.Name, StringComparer.Ordinal)
            .ToList();
    }

    static IReadOnlyList<EntityMember> FromClr(Type type)
    {
        var members = new List<EntityMember>();
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!IsPublic(property) ||
                property.GetIndexParameters().Length != 0)
            {
                continue;
            }

            if (TryScalar(property.PropertyType, out var scalar))
            {
                members.Add(new(property.Name, scalar, MemberKind.Scalar));
            }
        }

        return members
            .OrderBy(_ => _.Name, StringComparer.Ordinal)
            .ToList();
    }

    static bool IsPublic(PropertyInfo? property) =>
        property?.GetMethod is { IsPublic: true };

    static bool TryScalar(Type type, [NotNullWhen(true)] out Type? scalar)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        if (scalarTypes.Contains(underlying) ||
            underlying.IsEnum)
        {
            scalar = underlying;
            return true;
        }

        scalar = null;
        return false;
    }
}
