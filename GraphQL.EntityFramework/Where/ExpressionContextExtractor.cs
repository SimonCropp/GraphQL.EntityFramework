using System;
using System.Collections.Generic;
using System.Linq;

static class ExpressionContextExtractor
{
    public static IEnumerable<WhereExpression> ReadWhere(Func<Type, string, object> getArgument)
    {
        foreach (var expression in getArgument.ReadList<WhereExpression>("where"))
        {
            yield return expression;
        }
    }

    public static bool TryReadId(Func<Type, string, object> getArgument, out IdExpression expression)
    {
        if (getArgument.TryRead("id", out expression))
        {
            //TODO: validate
            return true;
        }
        return false;
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