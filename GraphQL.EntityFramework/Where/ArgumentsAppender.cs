using System.Collections.Generic;
using GraphQL.Types;

static class ArgumentAppender
{
    static QueryArgument<ListGraphType<WhereExpressionGraph>> whereArgument =
        new QueryArgument<ListGraphType<WhereExpressionGraph>>
        {
            Name = "where"
        };

    static QueryArgument<ListGraphType<OrderByGraph>> orderByArgument =
        new QueryArgument<ListGraphType<OrderByGraph>>
        {
            Name = "orderBy"
        };

    static QueryArgument<ListGraphType<StringGraphType>> idArgument =
        new QueryArgument<ListGraphType<StringGraphType>>
        {
            Name = "ids"
        };

    static QueryArgument<IntGraphType> skipArgument =
        new QueryArgument<IntGraphType>
        {
            Name = "skip"
        };

    static QueryArgument<IntGraphType> takeArgument =
        new QueryArgument<IntGraphType>
        {
            Name = "take"
        };

    public static void AddWhereArgument(this FieldType field, IEnumerable<QueryArgument> extra)
    {
        var arguments = field.Arguments;
        arguments.Add(whereArgument);
        arguments.Add(orderByArgument);
        arguments.Add(idArgument);
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
            idArgument,
            orderByArgument,
            whereArgument,
            skipArgument,
            takeArgument
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