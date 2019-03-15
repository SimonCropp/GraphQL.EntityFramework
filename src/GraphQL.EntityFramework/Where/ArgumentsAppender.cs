using System.Collections.Generic;
using GraphQL.EntityFramework;
using GraphQL.Types;

static class ArgumentAppender
{
    static QueryArgument<ListGraphType<WhereExpressionGraph>> whereArgument()
    {
        return new QueryArgument<ListGraphType<WhereExpressionGraph>>
        {
            Name = "where"
        };
    }

    static QueryArgument<ListGraphType<OrderByGraph>> orderByArgument()
    {
        return new QueryArgument<ListGraphType<OrderByGraph>>
        {
            Name = "orderBy"
        };
    }

    static QueryArgument<ListGraphType<StringGraphType>> idsArgument()
    {
        return new QueryArgument<ListGraphType<StringGraphType>>
        {
            Name = "ids"
        };
    }

    static QueryArgument<StringGraphType> idArgument()
    {
        return new QueryArgument<StringGraphType>
        {
            Name = "id"
        };
    }

    static QueryArgument<IntGraphType> skipArgument()
    {
        return new QueryArgument<IntGraphType>
        {
            Name = "skip"
        };
    }

    static QueryArgument<IntGraphType> takeArgument()
    {
        return new QueryArgument<IntGraphType>
        {
            Name = "take"
        };
    }

    public static void AddWhereArgument(this FieldType field, IEnumerable<QueryArgument> extra)
    {
        var arguments = field.Arguments;
        arguments.Add(whereArgument());
        arguments.Add(orderByArgument());
        arguments.Add(idsArgument());
        if (extra != null)
        {
            foreach (var argument in extra)
            {
                arguments.Add(argument);
            }
        }
    }

    public static QueryArguments GetQueryArguments(IEnumerable<QueryArgument> extra)
    {
        var arguments = new QueryArguments
        {
            idArgument(),
            idsArgument(),
            orderByArgument(),
            whereArgument(),
            skipArgument(),
            takeArgument()
        };
        if (extra != null)
        {
            foreach (var argument in extra)
            {
                arguments.Add(argument);
            }
        }

        return arguments;
    }
}