using GraphQL;

static class GraphTypeFinder
{
    public static Type FindGraphType<TReturn>(bool isNullable = false)
        where TReturn : class
    {
        var type = typeof(TReturn);
        return FindGraphType(type, isNullable);
    }

    public static Type FindGraphType(Type type, bool isNullable = false) =>
        type.GetGraphTypeFromType(isNullable, TypeMappingMode.OutputType);
}