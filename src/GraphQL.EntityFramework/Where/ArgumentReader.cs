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
        // Get the "ids" as an object (GraphQL short-circuits the GetArgument to stop when we request an object)
        // This lets us determine what type of array we're working with (if any)
        var argument = getArgument(typeof(object), "ids");
        if (argument == null)
        {
            expression = null;
            return false;
        }

        if (argument is List<object> lo)
            expression = lo.Select(o => o.ToString()).ToArray();
        else if (argument is object[] oa)
            expression = oa.Select(o => o.ToString()).ToArray();
        else
            throw new InvalidOperationException($"TryReadIds got an 'ids' argument of type '{argument.GetType().FullName}' which is unhandled.");

        return true;
    }

    public static bool TryReadId(Func<Type, string, object> getArgument, out string expression)
    {
        var argument = getArgument(typeof(object), "id");
        if (argument == null)
        {
            expression = null;
            return false;
        }

        if (argument is long l)
            expression = l.ToString(CultureInfo.InvariantCulture);
        else if (argument is int i)
            expression = i.ToString(CultureInfo.InvariantCulture);
        else if (argument is string s)
            expression = s;
        else
            throw new InvalidOperationException($"TryReadId got an 'id' argument of type '{argument.GetType().FullName}' which is unhandled.");

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