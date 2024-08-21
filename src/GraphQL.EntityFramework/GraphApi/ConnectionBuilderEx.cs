namespace GraphQL.EntityFramework;

class ConnectionBuilderEx<TSourceType> : ConnectionBuilder<TSourceType>
{
    Func<IResolveConnectionContext<TSourceType>, Task<object?>>? asyncResolver;
    Func<IResolveConnectionContext<TSourceType>, object?>? syncResolver;

    public ConnectionBuilderEx(FieldType fieldType) : base(fieldType) =>
        Bidirectional();

    public override ConnectionBuilder<TSourceType> Resolve(Func<IResolveConnectionContext<TSourceType>, object?> resolver)
    {
        asyncResolver = null;
        syncResolver = resolver;
        return base.Resolve(resolver);
    }

    public override ConnectionBuilder<TSourceType> ResolveAsync(Func<IResolveConnectionContext<TSourceType>, Task<object?>> resolver)
    {
        asyncResolver = resolver;
        syncResolver = null;
        return base.ResolveAsync(resolver);
    }

    public override ConnectionBuilder<TSourceType> PageSize(int pageSize)
    {
        base.PageSize(pageSize);
        ResetResolver();
        return this;
    }

    void ResetResolver()
    {
        if (asyncResolver != null)
        {
            ResolveAsync(asyncResolver);
        }

        if (syncResolver != null)
        {
            Resolve(syncResolver);
        }
    }

    public static ConnectionBuilderEx<TSourceType> Build<TNodeType>(string name)
        where TNodeType : IGraphType
    {
        var field = new FieldType
        {
            Name = name,
            Type = typeof(ConnectionType<TNodeType, EdgeType<TNodeType>>),
            Arguments = new(
                new QueryArgument(typeof(StringGraphType))
                {
                    Name = "after",
                    Description = "Only return edges after the specified cursor.",
                },
                new QueryArgument(typeof(IntGraphType))
                {
                    Name = "first",
                    Description = "Specifies the maximum number of edges to return, starting after the cursor specified by 'after', or the first number of edges if 'after' is not specified.",
                }),
        };
        return new(field);
    }
}