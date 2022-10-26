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

    static QueryArgument<ListGraphType<NonNullGraphType<IdGraph>>> IdsArgument() =>
        new()
        {
            Name = "ids"
        };

    static QueryArgument<IdGraph> IdArgument() =>
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

    public static void AddWhereArgument(this FieldType field, bool hasId)
    {
        var arguments = field.Arguments!;
        arguments.Add(WhereArgument());
        arguments.Add(OrderByArgument());
        if (hasId)
        {
            arguments.Add(IdsArgument());
        }
    }

    public static QueryArguments GetQueryArguments(bool hasId, bool applyOrder)
    {
        var arguments = new QueryArguments();
        if (hasId)
        {
            arguments.Add(IdArgument());
            arguments.Add(IdsArgument());
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