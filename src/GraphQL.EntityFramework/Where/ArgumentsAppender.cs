using System.Collections.Generic;
using GraphQL.EntityFramework;
using GraphQL.Types;

static class ArgumentAppender
{
    static QueryArgument<ListGraphType<NonNullGraphType<WhereExpressionGraph>>> whereArgument()
    {
        return new()
        {
            Name = "where"
        };
    }

    static QueryArgument<ListGraphType<NonNullGraphType<OrderByGraph>>> orderByArgument()
    {
        return new()
        {
            Name = "orderBy"
        };
    }

    static QueryArgument<ListGraphType<NonNullGraphType<IdGraphType>>> idsArgument()
    {
        return new()
        {
            Name = "ids"
        };
    }

    static QueryArgument<IdGraphType> idArgument()
    {
        return new()
        {
            Name = "id"
        };
    }

    static QueryArgument<IntGraphType> skipArgument()
    {
        return new()
        {
            Name = "skip"
        };
    }

    static QueryArgument<IntGraphType> takeArgument()
    {
        return new()
        {
            Name = "take"
        };
    }

    public static void AddWhereArgument(this FieldType field, bool hasId, IEnumerable<QueryArgument>? extra)
    {
        var arguments = field.Arguments!;
        arguments.Add(whereArgument());
        arguments.Add(orderByArgument());
        if (hasId)
        {
            arguments.Add(idsArgument());
        }
        if (extra is not null)
        {
            foreach (var argument in extra)
            {
                arguments.Add(argument);
            }
        }
    }

    public static QueryArguments GetQueryArguments(IEnumerable<QueryArgument>? extra, bool hasId, bool applyOrder)
    {
        QueryArguments arguments = new();
        if (hasId)
        {
            arguments.Add(idArgument());
            arguments.Add(idsArgument());
        }

        arguments.Add(whereArgument());
        if (applyOrder)
        {
            arguments.Add(orderByArgument());
            arguments.Add(skipArgument());
            arguments.Add(takeArgument());
        }

        if (extra is not null)
        {
            foreach (var argument in extra)
            {
                arguments.Add(argument);
            }
        }

        return arguments;
    }
}