using System;
using System.Collections.Generic;
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
        var argument = (string[])getArgument(typeof(string[]), "ids");
        if (argument == null)
        {
            expression = null;
            return false;
        }

        expression = argument;

        return true;
    }

    public static bool TryReadId(Func<Type, string, object> getArgument, out string expression)
    {
        var argument = (string)getArgument(typeof(string), "id");
        if (argument == null)
        {
            expression = null;
            return false;
        }

        expression = argument;

        return true;
    }

    public static bool TryReadSkip(Func<Type, string, object> getArgument, out int skip)
    {
        var result = getArgument.TryRead("skip", out skip);
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
        var result = getArgument.TryRead("take", out take);
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

    static bool TryRead<T>(this Func<Type, string, object> getArgument, string name, out T value)
    {
        var argument = getArgument(typeof(T), name);
        if (argument == null)
        {
            value = default;
            return false;
        }

        value = (T) argument;
        return true;
    }
}