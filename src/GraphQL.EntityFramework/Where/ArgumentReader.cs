using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using GraphQL.EntityFramework;

static class ArgumentReader
{
    public static bool TryReadWhere(Func<Type, string, object?> getArgument, out WhereExpression[] expression)
    {
        expression = getArgument.ReadList<WhereExpression>("where");
        return expression.Any();
    }

    public static OrderBy[] ReadOrderBy(Func<Type, string, object?> getArgument)
    {
        return getArgument.ReadList<OrderBy>("orderBy");
    }

    public static bool TryReadIds(Func<Type, string, object?> getArgument, [NotNullWhen(returnValue: true)] out string[]? expression)
    {
        var argument = getArgument(typeof(object), "ids");
        if (argument == null)
        {
            expression = null;
            return false;
        }

        if (argument is IEnumerable<object> objCollection)
        {
            expression = objCollection.Select(o => o.ToString()).ToArray()!;
            return true;
        }

        throw new($"TryReadIds got an 'ids' argument of type '{argument.GetType().FullName}' which is not supported.");
    }

    public static bool TryReadId(Func<Type, string, object?> getArgument, [NotNullWhen(returnValue: true)] out string? expression)
    {
        var argument = getArgument(typeof(object), "id");
        if (argument == null)
        {
            expression = null;
            return false;
        }

        switch (argument)
        {
            case long l:
                expression = l.ToString(CultureInfo.InvariantCulture);
                break;
            case int i:
                expression = i.ToString(CultureInfo.InvariantCulture);
                break;
            case string s:
                expression = s;
                break;
            default:
                throw new($"TryReadId got an 'id' argument of type '{argument.GetType().FullName}' which is not supported.");
        }

        return true;
    }

    public static bool TryReadSkip(Func<Type, string, object?> getArgument, out int skip)
    {
        var result = getArgument.TryReadInt("skip", out skip);
        if (result)
        {
            if (skip < 0)
            {
                throw new("Skip cannot be less than 0.");
            }
        }
        return result;
    }

    public static bool TryReadTake(Func<Type, string, object?> getArgument, out int take)
    {
        var result = getArgument.TryReadInt("take", out take);
        if (result)
        {
            if (take < 0)
            {
                throw new("Take cannot be less than 0.");
            }
        }
        return result;
    }

    static T[] ReadList<T>(this Func<Type, string, object?> getArgument, string name)
    {
        var argument = getArgument(typeof(T[]), name);
        if (argument == null)
        {
            return Array.Empty<T>();
        }

        return (T[]) argument;
    }

    static bool TryReadInt(this Func<Type, string, object?> getArgument, string name, out int value)
    {
        var argument = getArgument(typeof(int), name);
        if (argument == null)
        {
            value = 0;
            return false;
        }

        value = (int)argument;
        return true;
    }
}