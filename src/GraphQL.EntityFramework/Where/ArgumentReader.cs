static class ArgumentReader
{
    public static bool TryReadWhere(IResolveFieldContext context, out IEnumerable<WhereExpression> expression)
    {
        expression = ReadList<WhereExpression>(context, "where");

        return expression.Any();
    }

    public static IEnumerable<OrderBy> ReadOrderBy(IResolveFieldContext context) =>
        ReadList<OrderBy>(context, "orderBy");

    public static bool TryReadIds(IResolveFieldContext context, [NotNullWhen(true)] out string[]? result)
    {
        static string ArgumentToExpression(object argument) =>
            argument switch
            {
                long l => l.ToString(CultureInfo.InvariantCulture),
                int i => i.ToString(CultureInfo.InvariantCulture),
                string s => s,
                _ => throw new($"TryReadId got an 'id' argument of type '{argument.GetType().FullName}' which is not supported.")
            };

        var arguments = context.Arguments;
        if (arguments == null)
        {
            result = null;
            return false;
        }

        var containsIds = arguments.TryGetValue("ids", out var ids);
        var containsId = arguments.TryGetValue("id", out var id);

        if (!containsIds && !containsId)
        {
            result = null;
            return false;
        }

        if (ids.Source == ArgumentSource.FieldDefault &&
            id.Source == ArgumentSource.FieldDefault)
        {
            result = null;
            return false;
        }

        var expressions = new List<string>();

        if (id.Source != ArgumentSource.FieldDefault)
        {
            var idValue = id.Value;
            if (idValue == null)
            {
                throw new("Null 'id' is not supported.");
            }

            expressions.Add(ArgumentToExpression(idValue));
        }

        if (ids.Source != ArgumentSource.FieldDefault)
        {
            if (ids.Value is not IEnumerable<object> objCollection)
            {
                throw new($"TryReadIds got an 'ids' argument of type '{ids.Value!.GetType().FullName}' which is not supported.");
            }

            expressions.AddRange(objCollection.Select(ArgumentToExpression));
        }

        result = expressions.ToArray();
        return true;
    }

    public static bool TryReadSkip(IResolveFieldContext context, out int skip)
    {
        var result = TryReadInt("skip", context, out skip);
        if (result)
        {
            if (skip < 0)
            {
                throw new("Skip cannot be less than 0.");
            }
        }

        return result;
    }

    public static bool TryReadTake(IResolveFieldContext context, out int take)
    {
        var result = TryReadInt("take", context, out take);
        if (result)
        {
            if (take < 0)
            {
                throw new("Take cannot be less than 0.");
            }
        }

        return result;
    }

    static IEnumerable<T> ReadList<T>(IResolveFieldContext context, string name)
    {
        var argument = context.GetArgument(typeof(T[]), name);
        if (argument is null)
        {
            return Enumerable.Empty<T>();
        }

        return (T[]) argument;
    }

    static bool TryReadInt(string name, IResolveFieldContext context, out int value)
    {
        var argument = context.GetArgument(typeof(int), name);
        if (argument is null)
        {
            value = 0;
            return false;
        }

        value = (int) argument;
        return true;
    }
}