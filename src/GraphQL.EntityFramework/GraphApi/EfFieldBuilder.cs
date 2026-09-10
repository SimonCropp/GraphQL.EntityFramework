using GraphQL.Instrumentation;
using GraphQL.Validation;

namespace GraphQL.EntityFramework;

/// <summary>
/// A <see cref="FieldBuilder{TSourceType, TReturnType}"/> that carries the <see cref="IEfGraphQLService{TDbContext}"/>,
/// so projection-based resolvers can be configured without passing the service.
/// Returned by the Field methods of <see cref="EfObjectGraphType{TDbContext, TSource}"/>
/// and <see cref="EfInterfaceGraphType{TDbContext, TSource}"/>.
/// The fluent methods are overridden to return this type so the projection-based
/// methods remain available anywhere in a chain.
/// </summary>
public class EfFieldBuilder<TDbContext, TSource, TReturn>(FieldType fieldType, IEfGraphQLService<TDbContext> graphQlService) :
    FieldBuilder<TSource, TReturn>(fieldType)
    where TDbContext : DbContext
{
    public IEfGraphQLService<TDbContext> GraphQlService { get; } = graphQlService;

    /// <summary>
    /// Resolves field value using a projection to ensure required data is loaded.
    /// </summary>
    /// <typeparam name="TProjection">The projected data type</typeparam>
    /// <param name="projection">Expression to project required data from source</param>
    /// <param name="resolve">Function to resolve field value from projected data</param>
    public EfFieldBuilder<TDbContext, TSource, TReturn> Resolve<TProjection>(
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, TReturn> resolve)
    {
        this.Resolve(GraphQlService, projection, resolve);
        return this;
    }

    /// <summary>
    /// Resolves field value asynchronously using a projection to ensure required data is loaded.
    /// </summary>
    /// <typeparam name="TProjection">The projected data type</typeparam>
    /// <param name="projection">Expression to project required data from source</param>
    /// <param name="resolve">Async function to resolve field value from projected data</param>
    public EfFieldBuilder<TDbContext, TSource, TReturn> ResolveAsync<TProjection>(
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, Task<TReturn>> resolve)
    {
        this.ResolveAsync(GraphQlService, projection, resolve);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Type(IGraphType type)
    {
        base.Type(type);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Description(string? description)
    {
        base.Description(description);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> DeprecationReason(string? deprecationReason)
    {
        base.DeprecationReason(deprecationReason);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> DefaultValue(TReturn? defaultValue = default)
    {
        base.DefaultValue(defaultValue);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> ParseValue(Func<object, object> parseValue)
    {
        base.ParseValue(parseValue);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Validate(Action<object> validation)
    {
        base.Validate(validation);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> ValidateArguments(Action<FieldArgumentsValidationContext> validation)
    {
        base.ValidateArguments(validation);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> ValidateArguments(Func<FieldArgumentsValidationContext, ValueTask> validation)
    {
        base.ValidateArguments(validation);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Resolve(IFieldResolver? resolver)
    {
        base.Resolve(resolver);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Resolve(Func<IResolveFieldContext<TSource>, TReturn?> resolve)
    {
        base.Resolve(resolve);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> ResolveAsync(Func<IResolveFieldContext<TSource>, Task<TReturn?>> resolve)
    {
        base.ResolveAsync(resolve);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> ResolveDelegate(Delegate? resolve)
    {
        base.ResolveDelegate(resolve);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Argument<TArgumentGraphType>(string name, string? description, Action<QueryArgument>? configure = null)
    {
        base.Argument<TArgumentGraphType>(name, description, configure);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Argument<TArgumentGraphType>(string name)
    {
        base.Argument<TArgumentGraphType>(name);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Argument<TArgumentGraphType>(string name, Action<QueryArgument>? configure = null)
    {
        base.Argument<TArgumentGraphType>(name, configure);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Argument<TArgumentClrType>(string name, bool nullable = false, Action<QueryArgument>? configure = null)
    {
        base.Argument<TArgumentClrType>(name, nullable, configure);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Argument<TArgumentClrType>(string name, bool nullable, string? description, Action<QueryArgument>? configure = null)
    {
        base.Argument<TArgumentClrType>(name, nullable, description, configure);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Argument(Type type, string name, Action<QueryArgument>? configure = null)
    {
        base.Argument(type, name, configure);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Argument(IGraphType type, string name, Action<QueryArgument>? configure = null)
    {
        base.Argument(type, name, configure);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Arguments(IEnumerable<QueryArgument> arguments)
    {
        base.Arguments(arguments);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Arguments(params QueryArgument[] arguments)
    {
        base.Arguments(arguments);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> Configure(Action<FieldType> configure)
    {
        base.Configure(configure);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> ResolveStream(Func<IResolveFieldContext<TSource>, IObservable<TReturn?>> sourceStreamResolver)
    {
        base.ResolveStream(sourceStreamResolver);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> ResolveStreamAsync(Func<IResolveFieldContext<TSource>, Task<IObservable<TReturn?>>> sourceStreamResolver)
    {
        base.ResolveStreamAsync(sourceStreamResolver);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> DependsOn<TService>()
    {
        base.DependsOn<TService>();
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> ApplyMiddleware(IFieldMiddleware middleware)
    {
        base.ApplyMiddleware(middleware);
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> ApplyMiddleware<TMiddleware>()
    {
        base.ApplyMiddleware<TMiddleware>();
        return this;
    }

    public override EfFieldBuilder<TDbContext, TSource, TReturn> ApplyMiddleware(Func<FieldMiddlewareDelegate, FieldMiddlewareDelegate> middleware)
    {
        base.ApplyMiddleware(middleware);
        return this;
    }
}
