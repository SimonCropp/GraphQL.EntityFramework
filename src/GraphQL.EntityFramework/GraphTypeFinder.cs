using System;
using GraphQL;

static class GraphTypeFinder
{
    public static Type FindGraphType<TReturn>()
        where TReturn : class
    {
        var type = typeof(TReturn);
        return FindGraphType(type);
    }

    public static Type FindGraphType(Type type, bool isNullable = false)
    {
        return type.GetGraphTypeFromType(isNullable, TypeMappingMode.OutputType);
    }
}