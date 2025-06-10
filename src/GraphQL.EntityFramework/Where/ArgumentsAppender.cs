using System.Numerics;

static class ArgumentAppender
{
    static QueryArgument<ListGraphType<NonNullGraphType<WhereExpressionGraph>>> WhereArgument() =>
        new()
        {
            Name = "where"
        };

    static QueryArgument<ListGraphType<NonNullGraphType<OrderByGraph>>> OrderByArgument() =>
        new()
        {
            Name = "orderBy"
        };

    static QueryArgument IdsArgument(Type key) =>
        new(GetNonNullableListKeyType(key))
        {
            Name = "ids"
        };

    static Type GetNonNullableListKeyType(Type key)
    {
        if (key == typeof(ushort))
        {
            return typeof(ListGraphType<NonNullGraphType<UShortGraphType>>);
        }

        if (key == typeof(uint))
        {
            return typeof(ListGraphType<NonNullGraphType<UIntGraphType>>);
        }

        if (key == typeof(ulong))
        {
            return typeof(ListGraphType<NonNullGraphType<ULongGraphType>>);
        }

        if (key == typeof(short))
        {
            return typeof(ListGraphType<NonNullGraphType<ShortGraphType>>);
        }

        if (key == typeof(int))
        {
            return typeof(ListGraphType<NonNullGraphType<IntGraphType>>);
        }

        if (key == typeof(long))
        {
            return typeof(ListGraphType<NonNullGraphType<LongGraphType>>);
        }

        if (key == typeof(string))
        {
            return typeof(ListGraphType<NonNullGraphType<StringGraphType>>);
        }

        if (key == typeof(Guid))
        {
            return typeof(ListGraphType<NonNullGraphType<GuidGraphType>>);
        }

        if (key == typeof(BigInteger))
        {
            return typeof(ListGraphType<NonNullGraphType<BigIntGraphType>>);
        }

        throw new($"Unsupported key type: {key.FullName}");
    }

    static QueryArgument IdArgumentNullable(Type key) =>
        new(GetNullableKeyType(key))
        {
            Name = "id"
        };

    static Type GetNonNullableKeyType(Type key)
    {
        if (key == typeof(ushort))
        {
            return typeof(NonNullGraphType<UShortGraphType>);
        }

        if (key == typeof(uint))
        {
            return typeof(NonNullGraphType<UIntGraphType>);
        }

        if (key == typeof(ulong))
        {
            return typeof(NonNullGraphType<ULongGraphType>);
        }

        if (key == typeof(short))
        {
            return typeof(NonNullGraphType<ShortGraphType>);
        }

        if (key == typeof(int))
        {
            return typeof(NonNullGraphType<IntGraphType>);
        }

        if (key == typeof(long))
        {
            return typeof(NonNullGraphType<LongGraphType>);
        }

        if (key == typeof(string))
        {
            return typeof(NonNullGraphType<StringGraphType>);
        }

        if (key == typeof(Guid))
        {
            return typeof(NonNullGraphType<GuidGraphType>);
        }

        if (key == typeof(BigInteger))
        {
            return typeof(NonNullGraphType<BigIntGraphType>);
        }

        throw new($"Unsupported key type: {key.FullName}");
    }

    static Type GetNullableKeyType(Type key)
    {
        if (key == typeof(ushort))
        {
            return typeof(UShortGraphType);
        }

        if (key == typeof(uint))
        {
            return typeof(UIntGraphType);
        }

        if (key == typeof(ulong))
        {
            return typeof(ULongGraphType);
        }

        if (key == typeof(short))
        {
            return typeof(ShortGraphType);
        }

        if (key == typeof(int))
        {
            return typeof(IntGraphType);
        }

        if (key == typeof(long))
        {
            return typeof(LongGraphType);
        }

        if (key == typeof(string))
        {
            return typeof(StringGraphType);
        }

        if (key == typeof(Guid))
        {
            return typeof(GuidGraphType);
        }

        if (key == typeof(BigInteger))
        {
            return typeof(BigIntGraphType);
        }

        throw new($"Unsupported key type: {key.FullName}");
    }

    static QueryArgument IdArgumentNotNullable(Type key) =>
        new(GetNonNullableKeyType(key))
        {
            Name = "id"
        };

    static QueryArgument<IntGraphType> SkipArgument() =>
        new()
        {
            Name = "skip"
        };

    static QueryArgument<IntGraphType> TakeArgument() =>
        new()
        {
            Name = "take"
        };

    public static void AddWhereArgument(this FieldType field, Func<Key>? keyFunc)
    {
        var arguments = field.Arguments!;
        arguments.Add(WhereArgument());
        arguments.Add(OrderByArgument());
        if (keyFunc != null)
        {
            var key = keyFunc();
            arguments.Add(IdsArgument(key.Type));
        }
    }

    public static QueryArguments? GetQueryArguments(Func<Key>? keyFunc, bool applyOrder, bool idOnly, bool omitQueryArguments = false)
    {
        if (omitQueryArguments && idOnly)
        {
            throw new("omitQueryArguments and idOnly are mutually exclusive");
        }

        if (idOnly)
        {
            var key = keyFunc!();
            return [IdArgumentNotNullable(key.Type)];
        }

        if (omitQueryArguments)
        {
            return null;
        }

        var arguments = new QueryArguments();
        if (keyFunc != null)
        {
            var key = keyFunc();
            arguments.Add(IdArgumentNullable(key.Type));
            arguments.Add(IdsArgument(key.Type));
        }

        arguments.Add(WhereArgument());
        if (applyOrder)
        {
            arguments.Add(OrderByArgument());
            arguments.Add(SkipArgument());
            arguments.Add(TakeArgument());
        }

        return arguments;
    }
}