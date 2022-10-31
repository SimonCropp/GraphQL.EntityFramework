static class ArgumentReader
{
    public static bool TryReadWhere(Func<Type, string, object?> getArgument, IResolveFieldContext context, out IEnumerable<WhereExpression> expression)
    {
        expression = getArgument.ReadList<WhereExpression>(context, "where");

        return expression.Any();
    }

    public static IEnumerable<OrderBy> ReadOrderBy(Func<Type, string, object?> getArgument, IResolveFieldContext context) =>
        getArgument.ReadList<OrderBy>(context, "orderBy");

    public static bool TryReadIds(Func<Type, string, object?> getArgument, IResolveFieldContext context, [NotNullWhen(true)] out string[]? result)
    {
        string ArgumentToExpression(object argument)
        {
            return argument switch
            {
                long l => l.ToString(CultureInfo.InvariantCulture),
                int i => i.ToString(CultureInfo.InvariantCulture),
                string s => s,
                _ => throw new($"TryReadId got an 'id' argument of type '{argument.GetType().FullName}' which is not supported.")
            };
        }

        var idsArgument = getArgument(typeof(object), "ids");
        var idArgument = getArgument(typeof(object), "id");
        if (idsArgument is null && idArgument is null)
        {
            result = null;
            return false;
        }

        var expressions = new List<string>();

        if (idArgument is not null)
        {
            expressions.Add( ArgumentToExpression(idArgument));
        }

        if (idsArgument is not null)
        {
            if (idsArgument is not IEnumerable<object> objCollection)
            {
                throw new($"TryReadIds got an 'ids' argument of type '{idsArgument.GetType().FullName}' which is not supported.");
            }

            expressions.AddRange(objCollection.Select(ArgumentToExpression));
        }

        result = expressions.ToArray();
        return true;
    }

    public static bool TryReadSkip(Func<Type, string, object?> getArgument, IResolveFieldContext context, out int skip)
    {
        var result = getArgument.TryReadInt("skip", context, out skip);
        if (result)
        {
            if (skip < 0)
            {
                throw new("Skip cannot be less than 0.");
            }
        }
        return result;
    }

    public static bool TryReadTake(Func<Type, string, object?> getArgument, IResolveFieldContext context, out int take)
    {
        var result = getArgument.TryReadInt("take", context, out take);
        if (result)
        {
            if (take < 0)
            {
                throw new("Take cannot be less than 0.");
            }
        }
        return result;
    }

    static IEnumerable<T> ReadList<T>(this Func<Type, string, object?> getArgument, IResolveFieldContext context, string name)
    {
        var argument = getArgument(typeof(T[]), name);
        if (argument is null)
        {
            return Enumerable.Empty<T>();
        }

        return (T[]) argument;
    }

    static bool TryReadInt(this Func<Type, string, object?> getArgument, string name, IResolveFieldContext context, out int value)
    {
        var argument = getArgument(typeof(int), name);
        if (argument is null)
        {
            value = 0;
            return false;
        }

        value = (int)argument;
        return true;
    }
}