static class ArgumentAppender
{
    static QueryArgument WhereArgument(Type entityType) =>
        new(typeof(WhereGraph<>).MakeGenericType(entityType))
        {
            Name = "where"
        };

    static QueryArgument OrderByArgument(Type entityType, OrderByStyle style)
    {
        var element = style == OrderByStyle.Enum ? typeof(OrderByEnumGraph<>) : typeof(OrderByGraph<>);
        return new(typeof(ListGraphType<>).MakeGenericType(typeof(NonNullGraphType<>).MakeGenericType(element.MakeGenericType(entityType))))
        {
            Name = "orderBy"
        };
    }

    static QueryArgument<ListGraphType<NonNullGraphType<IdGraphType>>> IdsArgument() =>
        new()
        {
            Name = "ids"
        };

    static QueryArgument<IdGraphType> IdArgumentNullable() =>
        new()
        {
            Name = "id"
        };

    static QueryArgument<NonNullGraphType<IdGraphType>> IdArgumentNotNullable() =>
        new()
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

    public static void AddWhereArgument(this FieldType field, Type entityType, bool hasId, OrderByStyle orderByStyle)
    {
        var arguments = field.Arguments!;
        arguments.Add(WhereArgument(entityType));
        arguments.Add(OrderByArgument(entityType, orderByStyle));
        if (hasId)
        {
            arguments.Add(IdsArgument());
        }
    }

    public static QueryArguments? GetQueryArguments(Type entityType, bool hasId, bool applyOrder, bool idOnly, OrderByStyle orderByStyle, bool omitQueryArguments = false)
    {
        if (omitQueryArguments && idOnly)
        {
            throw new("omitQueryArguments and idOnly are mutually exclusive");
        }

        if (idOnly)
        {
            return [with(IdArgumentNotNullable())];
        }

        if (omitQueryArguments)
        {
            return null;
        }

        var arguments = new QueryArguments();
        if (hasId)
        {
            arguments.Add(IdArgumentNullable());
            arguments.Add(IdsArgument());
        }

        arguments.Add(WhereArgument(entityType));
        if (applyOrder)
        {
            arguments.Add(OrderByArgument(entityType, orderByStyle));
            arguments.Add(SkipArgument());
            arguments.Add(TakeArgument());
        }

        return arguments;
    }
}
