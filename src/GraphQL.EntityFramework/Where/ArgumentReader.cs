static class ArgumentReader
{
    public static bool TryReadWhere(IResolveFieldContext context, out IReadOnlyCollection<WhereExpression> expression)
    {
        // The where graph parses the argument into a WhereExpression tree. An empty where, {},
        // is no filter.
        if (TryReadArgument(context, "where", out var value) &&
            value is WhereExpression { IsEmpty: false } where)
        {
            expression = [where];
            return true;
        }

        expression = [];
        return false;
    }

    public static IReadOnlyCollection<OrderBy> ReadOrderBy(IResolveFieldContext context)
    {
        if (TryReadArgument(context, "orderBy", out var value) &&
            value is IEnumerable items)
        {
            return items.Cast<OrderBy>().ToList();
        }

        return [];
    }

    static bool TryReadArgument(IResolveFieldContext context, string name, out object? value)
    {
        if (context.Arguments is not null &&
            context.Arguments.TryGetValue(name, out var argument) &&
            argument.Source != ArgumentSource.FieldDefault)
        {
            value = argument.Value;
            return value is not null;
        }

        value = null;
        return false;
    }

    public static bool TryReadIds(IResolveFieldContext context, [NotNullWhen(true)] out string[]? idValues)
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
            idValues = null;
            return false;
        }

        var containsIds = arguments.TryGetValue("ids", out var ids);
        var containsId = arguments.TryGetValue("id", out var id);

        if (!containsIds && !containsId)
        {
            idValues = null;
            return false;
        }

        // A null ids, whether literal or from a variable, is the same as not passing it.
        // It was dereferenced for its type name in the error below.
        var hasIds = ids.Source != ArgumentSource.FieldDefault && ids.Value is not null;
        var hasId = id.Source != ArgumentSource.FieldDefault;
        if (!hasIds && !hasId)
        {
            idValues = null;
            return false;
        }

        var expressions = new List<string>();

        if (hasId)
        {
            var idValue = id.Value;
            if (idValue == null)
            {
                throw new("Null 'id' is not supported.");
            }

            expressions.Add(ArgumentToExpression(idValue));
        }

        if (hasIds)
        {
            if (ids.Value is not IEnumerable<object> objCollection)
            {
                throw new($"TryReadIds got an 'ids' argument of type '{ids.Value!.GetType().FullName}' which is not supported.");
            }

            expressions.AddRange(objCollection.Select(ArgumentToExpression));
        }

        idValues = expressions.ToArray();
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