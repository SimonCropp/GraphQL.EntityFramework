using GraphQL.Builders;
using GraphQL.Types;
using GraphQL.Types.Relay;

namespace GraphQL.EntityFramework;

class ConnectionBuilderEx<TSourceType> : ConnectionBuilder<TSourceType>
{
    Func<IResolveConnectionContext<TSourceType>, Task<object?>>? asyncResolver;
    Func<IResolveConnectionContext<TSourceType>, object?>? syncResolver;

    protected ConnectionBuilderEx(FieldType fieldType) : base(fieldType) =>
        Bidirectional();

    public override void Resolve(Func<IResolveConnectionContext<TSourceType>, object?> resolver)
    {
        asyncResolver = null;
        syncResolver = resolver;
        base.Resolve(resolver);
    }

    public override void ResolveAsync(Func<IResolveConnectionContext<TSourceType>, Task<object?>> resolver)
    {
        asyncResolver = resolver;
        syncResolver = null;
        base.ResolveAsync(resolver);
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

    public static ConnectionBuilderEx<TSourceType> Create2<TNodeType>(string name = "default")
        where TNodeType : IGraphType =>
        Create2<TNodeType, EdgeType<TNodeType>>(name);

    static ConnectionBuilderEx<TSourceType> Create2<TNodeType, TEdgeType>(string name = "default")
        where TNodeType : IGraphType
        where TEdgeType : EdgeType<TNodeType>
        => Create2<TNodeType, TEdgeType, ConnectionType<TNodeType, TEdgeType>>(name);

    static ConnectionBuilderEx<TSourceType> Create2<TNodeType, TEdgeType, TConnectionType>(string name = "default")
        where TNodeType : IGraphType
        where TEdgeType : EdgeType<TNodeType>
        where TConnectionType : ConnectionType<TNodeType, TEdgeType>
    {
        var fieldType = new FieldType
        {
            Name = name,
            Type = typeof(TConnectionType),
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
        return new(fieldType);
    }
}

public static class ConnectionBuilderEx
{
    /// <summary>
    /// Returns a builder for new connection field for the specified node type.
    /// The edge type is <see cref="EdgeType{TNodeType}">EdgeType</see>&lt;<typeparamref name="TNodeType"/>&gt;.
    /// The connection type is <see cref="ConnectionType{TNodeType, TEdgeType}">ConnectionType</see>&lt;<typeparamref name="TNodeType"/>, <see cref="EdgeType{TNodeType}">EdgeType</see>&lt;<typeparamref name="TNodeType"/>&gt;&gt;.
    /// </summary>
    /// <typeparam name="TNodeType">The graph type of the connection's node.</typeparam>
    /// <typeparam name="TSourceType">The type of <see cref="IResolveFieldContext.Source"/>.</typeparam>
    public static ConnectionBuilder<TSourceType> Create<TNodeType, TSourceType>()
        where TNodeType : IGraphType
        => ConnectionBuilderEx<TSourceType>.Create2<TNodeType>();
}