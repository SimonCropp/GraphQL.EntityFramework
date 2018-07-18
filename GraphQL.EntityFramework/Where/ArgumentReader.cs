using System;
using System.Collections.Generic;
using System.Linq;

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

    public static bool TryReadId(Func<Type, string, object> getArgument, out string[] expression)
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

    public static bool TryReadSkip(Func<Type, string, object> getArgument, out int skip)
    {
        return getArgument.TryRead("skip", out skip);
    }

    public static bool TryReadTake(Func<Type, string, object> getArgument, out int take)
    {
        return getArgument.TryRead("take", out take);
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