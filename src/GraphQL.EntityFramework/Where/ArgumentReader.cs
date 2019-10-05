using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GraphQL.EntityFramework;

static class ArgumentReader
{
    public static IEnumerable<WhereExpression> ReadWhere(Func<Type, string, object> getArgument)
    {
        return getArgument.ReadList<WhereExpression>("where");
    }

    public static IEnumerable<OrderBy> ReadOrderBy(Func<Type, string, object> getArgument)
    {
        return getArgument.ReadList<OrderBy>("orderBy");
    }

    public static bool TryReadIds(Func<Type, string, object> getArgument, out string[] expression)
    {
        var argument = getArgument(typeof(object), "ids");
        if (argument == null)
        {
            expression = Array.Empty<string>();
            return false;
        }

        if (argument is IEnumerable<object> objCollection)
        {
            expression = objCollection.Select(o => o.ToString()).ToArray();
            return true;
        }

        throw new Exception($"TryReadIds got an 'ids' argument of type '{argument.GetType().FullName}' which is not supported.");
    }

    public static bool TryReadId(Func<Type, string, object> getArgument, out string expression)
    {
        var argument = getArgument(typeof(object), "id");
        if (argument == null)
        {
            expression = string.Empty;
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
                throw new Exception($"TryReadId got an 'id' argument of type '{argument.GetType().FullName}' which is not supported.");
        }

        return true;
    }

    public static bool TryReadSkip(Func<Type, string, object> getArgument, out int skip)
    {
        var result = getArgument.TryReadInt("skip", out skip);
        if (result)
        {
            if (skip < 0)
            {
                throw new Exception("Skip cannot be less than 0.");
            }
        }
        return result;
    }

    public static bool TryReadTake(Func<Type, string, object> getArgument, out int take)
    {
        var result = getArgument.TryReadInt("take", out take);
        if (result)
        {
            if (take < 0)
            {
                throw new Exception("Take cannot be less than 0.");
            }
        }
        return result;
    }

    static IEnumerable<T> ReadList<T>(this Func<Type, string, object> getArgument, string name)
    {
        var argument = getArgument(typeof(T[]), name);
        if (argument == null)
        {
            return Enumerable.Empty<T>();
        }

        return (T[]) argument;
    }

    static bool TryReadInt(this Func<Type, string, object> getArgument, string name, out int value)
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